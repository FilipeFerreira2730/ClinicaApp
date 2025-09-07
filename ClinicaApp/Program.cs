using ClinicaApp.Components;
using ClinicaApp.Data;
using ClinicaApp.DTO;
using ClinicaApp.Services;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Adicionar DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registrar os serviços
builder.Services.AddScoped<SalaService>();
builder.Services.AddScoped<ProfissionalService>();
builder.Services.AddScoped<ReservaService>();
builder.Services.AddScoped<UserService>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// -----------------------------
// CRUD Salas
// -----------------------------
app.MapGet("/api/salas", async (SalaService service) =>
{
    var salas = await service.GetAllAsync();
    return salas.Any() ? Results.Ok(salas) : Results.Ok("Não existem salas cadastradas.");
});

app.MapGet("/api/salas/{id:int}", async (int id, SalaService service) =>
{
    var sala = await service.GetByIdAsync(id);
    return sala != null ? Results.Ok(sala) : Results.NotFound("Sala não existe.");
});

app.MapPost("/api/salas", async (Sala sala, SalaService service) =>
{
    await service.AddAsync(sala);
    return Results.Ok($"Sala '{sala.Nome}' criada com sucesso. ID: {sala.Id}");
});

app.MapPut("/api/salas/{id:int}", async (int id, Sala sala, SalaService service) =>
{
    var existing = await service.GetByIdAsync(id);
    if (existing == null) return Results.NotFound("Sala não existe.");

    existing.Nome = sala.Nome;
    await service.UpdateAsync(existing);
    return Results.Ok($"Sala '{existing.Nome}' editada com sucesso.");
});

app.MapDelete("/api/salas/{id:int}", async (int id, SalaService service) =>
{
    await service.DeleteAsync(id);
    return Results.Ok($"Sala ID {id} eliminada com sucesso.");
});

// -----------------------------
// CRUD Profissionais (DTO para GET, entidade para POST/PUT/DELETE)
// -----------------------------
app.MapGet("/api/profissionais", async (ProfissionalService service) =>
{
    var dtos = await service.GetAllAsync();
    return dtos.Any() ? Results.Ok(dtos) : Results.Ok("Não existem profissionais cadastrados.");
});

app.MapGet("/api/profissionais/{id:int}", async (int id, ProfissionalService service) =>
{
    var dto = await service.GetByIdAsync(id);
    return dto != null ? Results.Ok(dto) : Results.NotFound("Profissional não existe.");
});

app.MapPost("/api/profissionais", async (Profissional p, ProfissionalService service, UserService userService) =>
{
    var user = await userService.GetEntityByIdAsync(p.UserId);
    if (user == null) return Results.NotFound("User não existe.");

    if (user.RoleId != 1 && user.RoleId != 2)
        return Results.BadRequest("Apenas usuários com RoleId 1 ou 2 podem ser profissionais.");

    await service.AddAsync(p);
    return Results.Ok($"Profissional da especialidade '{p.Especialidade}' criado com sucesso. ID: {p.Id}");
});

app.MapPut("/api/profissionais/{id:int}", async (int id, Profissional p, ProfissionalService service) =>
{
    var entity = await service.GetEntityByIdAsync(id);
    if (entity == null) return Results.NotFound("Profissional não existe.");

    // Só altera a especialidade
    entity.Especialidade = p.Especialidade;

    await service.UpdateAsync(entity);
    return Results.Ok($"Profissional da especialidade '{entity.Especialidade}' editado com sucesso.");
});

app.MapDelete("/api/profissionais/{id:int}", async (int id, ProfissionalService service) =>
{
    await service.DeleteAsync(id);
    return Results.Ok($"Profissional ID {id} eliminado com sucesso.");
});

// -----------------------------
// CRUD Reservas
// -----------------------------
app.MapGet("/api/reservas", async (ReservaService service) =>
{
    var reservas = await service.GetAllAsync(); // devolve DTOs
    return reservas.Any() ? Results.Ok(reservas) : Results.Ok("Não existem reservas cadastradas.");
});

app.MapGet("/api/reservas/{id:int}", async (int id, ReservaService service) =>
{
    var r = await service.GetByIdAsync(id); // devolve DTO
    return r != null ? Results.Ok(r) : Results.NotFound("Reserva não existe.");
});

app.MapPost("/api/reservas", async (Reserva r, ReservaService service) =>
{
    await service.AddAsync(r);
    return Results.Ok($"Reserva criada com sucesso: Sala {r.SalaId}, Profissional {r.ProfissionalId}, ID: {r.Id}");
});

app.MapPut("/api/reservas/{id:int}", async (int id, Reserva r, ReservaService service) =>
{
    var existing = await service.GetEntityByIdAsync(id);
    if (existing == null) return Results.NotFound("Reserva não existe.");

    existing.SalaId = r.SalaId;
    existing.ProfissionalId = r.ProfissionalId;
    existing.DataHoraInicio = r.DataHoraInicio;
    existing.DataHoraFim = r.DataHoraFim;
    existing.UserId = r.UserId;

    await service.UpdateAsync(existing);
    return Results.Ok($"Reserva ID {existing.Id} editada com sucesso.");
});

app.MapDelete("/api/reservas/{id:int}", async (int id, ReservaService service) =>
{
    await service.DeleteAsync(id);
    return Results.Ok($"Reserva ID {id} eliminada com sucesso.");
});

// -----------------------------
// USERS (DTO para GET, entidade para POST/PUT/DELETE)
// -----------------------------
app.MapGet("/api/users", async (UserService service) =>
{
    var dtos = await service.GetAllAsync();
    return dtos.Any() ? Results.Ok(dtos) : Results.Ok("Não existem users cadastrados.");
});

app.MapGet("/api/users/{id:int}", async (int id, UserService service) =>
{
    var dto = await service.GetByIdAsync(id);
    return dto != null ? Results.Ok(dto) : Results.NotFound("User não existe.");
});

app.MapPost("/api/users", async (User u, UserService service) =>
{
    var validationContext = new ValidationContext(u);
    var validationResults = new List<ValidationResult>();
    if (!Validator.TryValidateObject(u, validationContext, validationResults, true))
        return Results.BadRequest(validationResults.Select(vr => vr.ErrorMessage));

    await service.AddAsync(u);
    return Results.Ok($"User '{u.Nome}' criado com sucesso. ID: {u.Id}");
});

// -----------------------------
// Invite (profissionais apenas)
// -----------------------------
app.MapPost("/api/users/invite", async (User u, UserService service) =>
{
    if (u.RoleId != 2)
        return Results.BadRequest("Este endpoint é apenas para criar profissionais (RoleId=2).");

    u.PasswordHash = null;
    u.PasswordSalt = null;

    await service.AddAsync(u);
    return Results.Ok($"Profissional '{u.Nome}' convidado com sucesso. ID: {u.Id}");
});

// -----------------------------
// Definir senha (usa entidade real, não DTO)
// -----------------------------
app.MapPost("/api/users/set-password", async (int userId, string password, UserService service) =>
{
    var userEntity = await service.GetEntityByIdAsync(userId);
    if (userEntity == null) return Results.NotFound("User não existe.");

    using var hmac = new HMACSHA512();
    userEntity.PasswordSalt = hmac.Key;
    userEntity.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

    await service.UpdateAsync(userEntity);
    return Results.Ok($"Password definida com sucesso para o user '{userEntity.Nome}'.");
});

app.MapPut("/api/users/{id:int}", async (int id, User u, UserService service) =>
{
    var entity = await service.GetEntityByIdAsync(id);
    if (entity == null) return Results.NotFound("User não existe.");

    entity.Nome = u.Nome;
    entity.Email = u.Email;
    entity.Telefone = u.Telefone;
    entity.RoleId = u.RoleId;

    await service.UpdateAsync(entity);
    return Results.Ok($"User '{entity.Nome}' editado com sucesso.");
});

app.MapDelete("/api/users/{id:int}", async (int id, UserService service) =>
{
    await service.DeleteAsync(id);
    return Results.Ok($"User ID {id} eliminado com sucesso.");
});

// -----------------------------
// Razor Components
// -----------------------------
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
