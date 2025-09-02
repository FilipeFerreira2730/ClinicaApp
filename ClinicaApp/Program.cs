using ClinicaApp.Components;
using ClinicaApp.Data;
using ClinicaApp.Services;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

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
// Minimal APIs CRUD para Salas
// -----------------------------
app.MapGet("/api/salas", async (SalaService service) => await service.GetAllAsync());
app.MapGet("/api/salas/{id:int}", async (int id, SalaService service) =>
{
    var sala = await service.GetByIdAsync(id);
    return sala is not null ? Results.Ok(sala) : Results.Ok("Sala não existe");
});
app.MapPost("/api/salas", async (Sala sala, SalaService service) =>
{
    await service.AddAsync(sala);
    return Results.Created($"/api/salas/{sala.Id}", sala);
});
app.MapPut("/api/salas/{id:int}", async (int id, Sala sala, SalaService service) =>
{
    var existingSala = await service.GetByIdAsync(id);
    if (existingSala is null)
        return Results.NotFound($"Sala com ID {id} não existe.");

    // Atualiza apenas os campos que existem
    existingSala.Nome = sala.Nome;

    await service.UpdateAsync(existingSala);
    return Results.NoContent();
});

app.MapDelete("/api/salas/{id:int}", async (int id, SalaService service) =>
{
    await service.DeleteAsync(id);
    return Results.NoContent();
});


// ---------------------------------
// Minimal APIs CRUD para Profissionais
// ---------------------------------
app.MapGet("/api/profissionais", async (ProfissionalService service) => await service.GetAllAsync());
app.MapGet("/api/profissionais/{id:int}", async (int id, ProfissionalService service) =>
{
    var p = await service.GetByIdAsync(id);
    return p is not null ? Results.Ok(p) : Results.Ok("Profissional não existe");
});
app.MapPost("/api/profissionais", async (Profissional p, ProfissionalService service) =>
{
    await service.AddAsync(p);
    return Results.Created($"/api/profissionais/{p.Id}", p);
});
app.MapPut("/api/profissionais/{id:int}", async (int id, Profissional p, ProfissionalService service) =>
{
    if (id != p.Id) return Results.BadRequest();
    await service.UpdateAsync(p);
    return Results.NoContent();
});
app.MapDelete("/api/profissionais/{id:int}", async (int id, ProfissionalService service) =>
{
    await service.DeleteAsync(id);
    return Results.NoContent();
});


// -----------------------------
// Minimal APIs CRUD para Reservas
// -----------------------------
app.MapGet("/api/reservas", async (ReservaService service) => await service.GetAllAsync());
app.MapGet("/api/reservas/{id:int}", async (int id, ReservaService service) =>
{
    var r = await service.GetByIdAsync(id);
    return r is not null ? Results.Ok(r) : Results.Ok("Reserva não existe");
});
app.MapPost("/api/reservas", async (Reserva r, ReservaService service) =>
{
    await service.AddAsync(r);
    return Results.Created($"/api/reservas/{r.Id}", r);
});
app.MapPut("/api/reservas/{id:int}", async (int id, Reserva r, ReservaService service) =>
{
    if (id != r.Id) return Results.BadRequest();
    await service.UpdateAsync(r);
    return Results.NoContent();
});
app.MapDelete("/api/reservas/{id:int}", async (int id, ReservaService service) =>
{
    await service.DeleteAsync(id);
    return Results.NoContent();
});


// -----------------------------
// Minimal APIs CRUD para Users
// -----------------------------
app.MapGet("/api/users", async (UserService service) => await service.GetAllAsync());
app.MapGet("/api/users/{id:int}", async (int id, UserService service) =>
{
    var u = await service.GetByIdAsync(id);
    return u is not null ? Results.Ok(u) : Results.Ok("User não existe");
});
app.MapPost("/api/users", async (User u, UserService service) =>
{
    var validationContext = new ValidationContext(u);
    var validationResults = new List<ValidationResult>();
    if (!Validator.TryValidateObject(u, validationContext, validationResults, true))
    {
        return Results.BadRequest(validationResults.Select(vr => vr.ErrorMessage));
    }

    await service.AddAsync(u);
    return Results.Created($"/api/users/{u.Id}", u);
});
app.MapPut("/api/users/{id:int}", async (int id, User u, UserService service) =>
{
    if (id != u.Id)
        return Results.BadRequest("O ID da rota não coincide com o ID do user.");

    var validationContext = new ValidationContext(u);
    var validationResults = new List<ValidationResult>();
    if (!Validator.TryValidateObject(u, validationContext, validationResults, true))
    {
        return Results.BadRequest(validationResults.Select(vr => vr.ErrorMessage));
    }

    await service.UpdateAsync(u);
    return Results.NoContent();
});
app.MapDelete("/api/users/{id:int}", async (int id, UserService service) =>
{
    await service.DeleteAsync(id);
    return Results.NoContent();
});


// -----------------------------
// Razor Components
// -----------------------------
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
