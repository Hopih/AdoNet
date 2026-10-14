using NpgsqlTypes;

namespace AdoNet.Data;
using Npgsql;

public class ProjectRepository(string connectionString) : BaseRepository(connectionString)
{
    public async Task<List<Project>> GetAllAsync(CancellationToken ct)
    {
        await using var connection = await OpenConnectionAsync(ct);

        // get_project_total_cost — наша SQL-функция (SqlScripts/create_function.sql)
        await using var command = new NpgsqlCommand(
            """
            SELECT p.id, p.user_id, u.name, p.address, p.start_date, p.budget,
                   public.get_project_total_cost(p.id) AS total_cost
            FROM public.projects p
            JOIN public.users u ON u.id = p.user_id
            ORDER BY p.start_date DESC, p.address
            """, connection);

        await using var reader = await command.ExecuteReaderAsync(ct);

        var projects = new List<Project>();
        while (await reader.ReadAsync(ct))
        {
            projects.Add(Map(reader));
        }

        return projects;
    }

    public async Task<Guid> CreateAsync(Project project, CancellationToken ct)
    {
        await using var connection = await OpenConnectionAsync(ct);

        await using var command = new NpgsqlCommand(
            """
            INSERT INTO public.projects (user_id, address, start_date, budget)
            VALUES (@user_id, @address, @start_date, @budget)
            RETURNING id
            """, connection);

        command.Parameters.AddWithValue("user_id", NpgsqlDbType.Uuid, project.UserId);
        command.Parameters.AddWithValue("address", NpgsqlDbType.Varchar, project.Address);
        command.Parameters.AddWithValue("start_date", NpgsqlDbType.Date, project.StartDate);
        command.Parameters.AddWithValue("budget", NpgsqlDbType.Numeric, project.Budget);

        return (Guid)(await command.ExecuteScalarAsync(ct))!;
    }

    public async Task UpdateAsync(Project project, CancellationToken ct)
    {
        await using var connection = await OpenConnectionAsync(ct);

        await using var command = new NpgsqlCommand(
            """
            UPDATE public.projects
            SET address = @address, start_date = @start_date, budget = @budget
            WHERE id = @id
            """, connection);

        command.Parameters.AddWithValue("id", NpgsqlDbType.Uuid, project.Id);
        command.Parameters.AddWithValue("address", NpgsqlDbType.Varchar, project.Address);
        command.Parameters.AddWithValue("start_date", NpgsqlDbType.Date, project.StartDate);
        command.Parameters.AddWithValue("budget", NpgsqlDbType.Numeric, project.Budget);

        await command.ExecuteNonQueryAsync(ct);
    }

    // Этапы проекта удалятся каскадно (ON DELETE CASCADE)
    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        await using var connection = await OpenConnectionAsync(ct);

        await using var command = new NpgsqlCommand("DELETE FROM public.projects WHERE id = @id", connection);

        command.Parameters.AddWithValue("id", NpgsqlDbType.Uuid, id);

        await command.ExecuteNonQueryAsync(ct);
    }

    private static Project Map(NpgsqlDataReader reader) => new()
    {
        Id = reader.GetGuid(0),
        UserId = reader.GetGuid(1),
        UserName = reader.GetString(2),
        Address = reader.GetString(3),
        StartDate = reader.GetFieldValue<DateOnly>(4),
        Budget = reader.GetDecimal(5),
        TotalCost = reader.GetDecimal(6)
    };
}
