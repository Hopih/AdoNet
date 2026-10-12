namespace AdoNet.Data;
using System.Data;
using Npgsql;

public class UserRepository(string connectionString):BaseRepository(connectionString)
{
    public async Task<Guid> CreateAsync(string title, CancellationToken ct)
    {
        await using var connection = await OpenConnectionAsync(ct);
        
        await using var command = new NpgsqlCommand("public.create_user", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        
        
    }
}