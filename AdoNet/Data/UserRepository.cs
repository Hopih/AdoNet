using NpgsqlTypes;

namespace AdoNet.Data;
using System.Data;
using Npgsql;

public class UserRepository(string connectionString):BaseRepository(connectionString)
{
    public async Task<List<User>> GetAllAsync(CancellationToken ct)
    {
        await using var connection = await OpenConnectionAsync(ct);

        await using var command = new NpgsqlCommand(
            "SELECT id, name, email, password FROM public.users ORDER BY name", connection);

        await using var reader = await command.ExecuteReaderAsync(ct);

        var users = new List<User>();
        while (await reader.ReadAsync(ct))
        {
            users.Add(Map(reader));
        }

        return users;
    }

    // Регистрация: вызывает хранимую процедуру create_user
    public async Task<Guid> CreateAsync(string name, string email, string password, CancellationToken ct)
    {
        await using var connection = await OpenConnectionAsync(ct);

        await using var command = new NpgsqlCommand("public.create_user", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("p_name",NpgsqlDbType.Varchar ,name);
        command.Parameters.AddWithValue("p_email",NpgsqlDbType.Varchar ,email);
        command.Parameters.AddWithValue("p_password", NpgsqlDbType.Varchar ,password);
        command.Parameters.Add("p_id", NpgsqlDbType.Uuid).Direction = ParameterDirection.Output;

        var result = await command.ExecuteScalarAsync(ct);

        return result is Guid id
            ? id
            : throw new InvalidOperationException("Процедура не вернула UUID");
    }

    public async Task UpdateAsync(Guid id, string name, string email, CancellationToken ct)
    {
        await using var connection = await OpenConnectionAsync(ct);

        await using var command = new NpgsqlCommand(
            "UPDATE public.users SET name = @name, email = @email WHERE id = @id", connection);

        command.Parameters.AddWithValue("id", NpgsqlDbType.Uuid, id);
        command.Parameters.AddWithValue("name", NpgsqlDbType.Varchar, name);
        command.Parameters.AddWithValue("email", NpgsqlDbType.Varchar, email);

        await command.ExecuteNonQueryAsync(ct);
    }

    // Вызывает хранимую процедуру delete_user (проекты и этапы удалятся каскадно)
    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        await using var connection = await OpenConnectionAsync(ct);

        await using var command = new NpgsqlCommand("public.delete_user", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("p_id", NpgsqlDbType.Uuid, id);

        await command.ExecuteNonQueryAsync(ct);
    }

    private static User Map(NpgsqlDataReader reader) => new()
    {
        Id = reader.GetGuid(0),
        Name = reader.GetString(1),
        Email = reader.GetString(2),
        Password = reader.GetString(3)
    };
}
