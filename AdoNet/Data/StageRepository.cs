using NpgsqlTypes;

namespace AdoNet.Data;
using Npgsql;

public class StageRepository(string connectionString) : BaseRepository(connectionString)
{
    public async Task<List<Stage>> GetAllAsync(CancellationToken ct)
    {
        await using var connection = await OpenConnectionAsync(ct);

        await using var command = new NpgsqlCommand(
            """
            SELECT s.id, s.project_id, p.address, s.title, s.cost, s.status
            FROM public.stages s
            JOIN public.projects p ON p.id = s.project_id
            ORDER BY p.address, s.title
            """, connection);

        await using var reader = await command.ExecuteReaderAsync(ct);

        var stages = new List<Stage>();
        while (await reader.ReadAsync(ct))
        {
            stages.Add(Map(reader));
        }

        return stages;
    }

    public async Task<Guid> CreateAsync(Stage stage, CancellationToken ct)
    {
        await using var connection = await OpenConnectionAsync(ct);

        // Транзакция нужна, потому что добавление этапа — это несколько запросов,
        // которые должны выполниться как одно целое:
        // 1) блокируем строку проекта (FOR UPDATE), чтобы два одновременных запроса
        //    не добавили этапы параллельно и вместе не превысили бюджет;
        // 2) вставляем этап;
        // 3) функцией get_project_total_cost считаем новую сумму всех этапов проекта.
        // Если сумма превысила бюджет — Rollback, и вставленный этап отменяется.
        // Без транзакции этап уже остался бы в БД, даже если бюджет превышен.
        // Если любой запрос упадёт с ошибкой, транзакция откатится сама при Dispose.
        await using var transaction = await connection.BeginTransactionAsync(ct);

        await using var lockCommand = new NpgsqlCommand(
            "SELECT budget FROM public.projects WHERE id = @project_id FOR UPDATE",
            connection, transaction);

        lockCommand.Parameters.AddWithValue("project_id", NpgsqlDbType.Uuid, stage.ProjectId);

        var budget = await lockCommand.ExecuteScalarAsync(ct) as decimal?
            ?? throw new InvalidOperationException("Проект не найден");

        await using var insertCommand = new NpgsqlCommand(
            """
            INSERT INTO public.stages (project_id, title, cost, status)
            VALUES (@project_id, @title, @cost, @status)
            RETURNING id
            """, connection, transaction);

        insertCommand.Parameters.AddWithValue("project_id", NpgsqlDbType.Uuid, stage.ProjectId);
        insertCommand.Parameters.AddWithValue("title", NpgsqlDbType.Varchar, stage.Title);
        insertCommand.Parameters.AddWithValue("cost", NpgsqlDbType.Numeric, stage.Cost);
        insertCommand.Parameters.AddWithValue("status", NpgsqlDbType.Varchar, stage.Status);

        var id = (Guid)(await insertCommand.ExecuteScalarAsync(ct))!;

        await using var totalCommand = new NpgsqlCommand(
            "SELECT public.get_project_total_cost(@project_id)", connection, transaction);

        totalCommand.Parameters.AddWithValue("project_id", NpgsqlDbType.Uuid, stage.ProjectId);

        var total = (decimal)(await totalCommand.ExecuteScalarAsync(ct))!;

        if (total > budget)
        {
            await transaction.RollbackAsync(ct);
            throw new InvalidOperationException(
                $"Этап не добавлен: сумма этапов {total} превысит бюджет проекта {budget}");
        }

        await transaction.CommitAsync(ct);

        return id;
    }

    public async Task UpdateAsync(Stage stage, CancellationToken ct)
    {
        await using var connection = await OpenConnectionAsync(ct);

        await using var command = new NpgsqlCommand(
            """
            UPDATE public.stages
            SET title = @title, cost = @cost, status = @status
            WHERE id = @id
            """, connection);

        command.Parameters.AddWithValue("id", NpgsqlDbType.Uuid, stage.Id);
        command.Parameters.AddWithValue("title", NpgsqlDbType.Varchar, stage.Title);
        command.Parameters.AddWithValue("cost", NpgsqlDbType.Numeric, stage.Cost);
        command.Parameters.AddWithValue("status", NpgsqlDbType.Varchar, stage.Status);

        await command.ExecuteNonQueryAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        await using var connection = await OpenConnectionAsync(ct);

        await using var command = new NpgsqlCommand("DELETE FROM public.stages WHERE id = @id", connection);

        command.Parameters.AddWithValue("id", NpgsqlDbType.Uuid, id);

        await command.ExecuteNonQueryAsync(ct);
    }

    private static Stage Map(NpgsqlDataReader reader) => new()
    {
        Id = reader.GetGuid(0),
        ProjectId = reader.GetGuid(1),
        ProjectAddress = reader.GetString(2),
        Title = reader.GetString(3),
        Cost = reader.GetDecimal(4),
        Status = reader.GetString(5)
    };
}
