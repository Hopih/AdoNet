using AdoNet.Data;
using Npgsql;
using NpgsqlTypes;

var user = new UserRepository(connectionString: "Host=localhost;Database=adonet;Username=postgres;Password=15092007qwe");
var id = await user.CreateAsync("test", "test@mail.ru", "123456", CancellationToken.None);
Console.WriteLine(id);

var command = Console.ReadLine();
if (command?.Trim() == "delete")
{
    await user.DeleteAsync(id, CancellationToken.None);
    Console.WriteLine($"Удалён {id}");
}
else
{
    Console.WriteLine($"Не удалено, ввёл: '{command}'");
}



