using NpgsqlTypes;

namespace AdoNet.Data;
using System.Data;
using Npgsql;

public class UserRepository(string connectionString):BaseRepository(connectionString)
{
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

        await command.ExecuteNonQueryAsync(ct);
        
        return Guid.NewGuid();
    }
}