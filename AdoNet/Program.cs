using AdoNet;
using AdoNet.Data;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default")!;
builder.Services.AddSingleton(new UserRepository(connectionString));
builder.Services.AddSingleton(new ProjectRepository(connectionString));
builder.Services.AddSingleton(new StageRepository(connectionString));

var app = builder.Build();

// Отдаёт wwwroot/index.html, style.css, app.js
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/users", (UserRepository users, CancellationToken ct) =>
    users.GetAllAsync(ct));
app.MapPost("/api/users", (User user, UserRepository users, CancellationToken ct) =>
    users.CreateAsync(user.Name, user.Email, user.Password, ct));
app.MapPut("/api/users/{id:guid}", (Guid id, User user, UserRepository users, CancellationToken ct) =>
    users.UpdateAsync(id, user.Name, user.Email, ct));
app.MapDelete("/api/users/{id:guid}", (Guid id, UserRepository users, CancellationToken ct) =>
    users.DeleteAsync(id, ct));

app.MapGet("/api/projects", (ProjectRepository projects, CancellationToken ct) =>
    projects.GetAllAsync(ct));
app.MapPost("/api/projects", (Project project, ProjectRepository projects, CancellationToken ct) =>
    projects.CreateAsync(project, ct));
app.MapPut("/api/projects/{id:guid}", (Guid id, Project project, ProjectRepository projects, CancellationToken ct) =>
{
    project.Id = id;
    return projects.UpdateAsync(project, ct);
});
app.MapDelete("/api/projects/{id:guid}", (Guid id, ProjectRepository projects, CancellationToken ct) =>
    projects.DeleteAsync(id, ct));

app.MapGet("/api/stages", (StageRepository stages, CancellationToken ct) =>
    stages.GetAllAsync(ct));
app.MapPost("/api/stages", async (Stage stage, StageRepository stages, CancellationToken ct) =>
{
    try
    {
        return Results.Ok(await stages.CreateAsync(stage, ct));
    }
    catch (InvalidOperationException ex)
    {
        // Транзакция откатилась (превышен бюджет) — отдаём причину на сайт
        return Results.Text(ex.Message, statusCode: 400);
    }
});
app.MapPut("/api/stages/{id:guid}", (Guid id, Stage stage, StageRepository stages, CancellationToken ct) =>
{
    stage.Id = id;
    return stages.UpdateAsync(stage, ct);
});
app.MapDelete("/api/stages/{id:guid}", (Guid id, StageRepository stages, CancellationToken ct) =>
    stages.DeleteAsync(id, ct));

app.Run();
