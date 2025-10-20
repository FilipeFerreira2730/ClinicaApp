using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using ClinicaApp;
using ClinicaApp.Components;
using ClinicaApp.Data;
using ClinicaApp.DTO;
using ClinicaApp.Converters;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------
// Serviços
// -----------------------------
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// HttpClient padrão para Blazor Server com BaseAddress da API
builder.Services.AddScoped(sp =>
{
    var nav = sp.GetRequiredService<NavigationManager>();
    return new HttpClient { BaseAddress = new Uri(nav.BaseUri + "api/") };
});

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.Converters.Add(new TimeSpanConverter());
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<SalaService>();
builder.Services.AddScoped<ProfissionalService>();
builder.Services.AddScoped<ReservaService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<EmailService>(); // <--- registado para envio de emails
builder.Services.AddScoped<HorarioService>();
builder.Services.AddSingleton<UserStateService>();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// -----------------------------
// Middleware
// -----------------------------
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
    return Results.Ok(salas);
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
// CRUD Profissionais
// -----------------------------
app.MapGet("/api/profissionais", async (ProfissionalService service) =>
{
    var dtos = await service.GetAllAsync();
    return Results.Ok(dtos);
});

app.MapGet("/api/profissionais/{id:int}", async (int id, ProfissionalService service) =>
{
    var dto = await service.GetByIdAsync(id);
    return dto != null ? Results.Ok(dto) : Results.NotFound("Profissional não existe.");
});

app.MapPost("/api/profissionais", async (Profissional p, ProfissionalService service, UserService userService, EmailService emailService) =>
{
    var user = await userService.GetEntityByIdAsync(p.UserId);
    if (user == null) return Results.NotFound("User não existe.");

    if (user.RoleId != 1 && user.RoleId != 2)
        return Results.BadRequest("Apenas usuários com RoleId 1 ou 2 podem ser profissionais.");

    // Adiciona o profissional
    await service.AddAsync(p);

    // Gerar token para definir a password
    user.PasswordResetToken = Guid.NewGuid().ToString();
    user.PasswordResetExpires = DateTime.UtcNow.AddHours(1);
    await userService.UpdateAsync(user);

    // Construir link para definir senha
    var resetLink = $"https://tua-app/AlterarPassword?userId={user.Id}&token={user.PasswordResetToken}";

    // Enviar email
    await emailService.SendEmailAsync(
        user.Email,
        "Defina a sua palavra-passe",
        $"Olá {user.Nome},<br><br>" +
        $"Clique no botão para definir a sua password:<br>" +
        $"<a href='{resetLink}' style='display:inline-block;padding:10px 20px;color:white;background-color:#007bff;text-decoration:none;border-radius:5px;'>Definir Password</a><br><br>" +
        $"Este link é válido por 1 hora."
    );

    return Results.Ok($"Profissional criado com sucesso. Email enviado para {user.Email}");
});


app.MapPut("/api/profissionais/{id:int}", async (int id, Profissional p, ProfissionalService service) =>
{
    var entity = await service.GetEntityByIdAsync(id);
    if (entity == null) return Results.NotFound("Profissional não existe.");

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
    var reservas = await service.GetAllAsync();
    return Results.Ok(reservas);
});

app.MapGet("/api/reservas/{id:int}", async (int id, ReservaService service) =>
{
    var r = await service.GetByIdAsync(id);
    return r != null ? Results.Ok(r) : Results.NotFound("Reserva não existe.");
});

app.MapPost("/api/reservas", async (Reserva r, ReservaService service) =>
{
    if (r.DataHoraFim <= r.DataHoraInicio)
        return Results.BadRequest("Data/hora de fim deve ser posterior ao início.");
    if (r.DataHoraInicio < DateTime.Now)
        return Results.BadRequest("Não é possível criar reservas retroativas.");

    // 1. Verificar sala
    var salaDisponivel = await service.IsDisponivelAsync(r.SalaId, r.DataHoraInicio, r.DataHoraFim);
    if (!salaDisponivel)
        return Results.Conflict("Este horário já está reservado para a sala selecionada.");

    // 2. Verificar profissional
    var profissionalDisponivel = await service.IsProfissionalDisponivelAsync(r.ProfissionalId, r.DataHoraInicio, r.DataHoraFim);
    if (!profissionalDisponivel)
        return Results.Conflict("Este profissional já tem uma reserva nesse horário noutra sala.");

    await service.AddAsync(r);
    return Results.Ok($"Reserva criada com sucesso: Sala {r.SalaId}, Profissional {r.ProfissionalId}, ID: {r.Id}");
});

app.MapPut("/api/reservas/{id:int}", async (int id, Reserva r, ReservaService service) =>
{
    var existing = await service.GetEntityByIdAsync(id);
    if (existing == null) return Results.NotFound("Reserva não existe.");

    if (r.DataHoraFim <= r.DataHoraInicio)
        return Results.BadRequest("Data/hora de fim deve ser posterior ao início.");
    if (r.DataHoraInicio < DateTime.Now)
        return Results.BadRequest("Não é possível criar reservas retroativas.");

    // 1. Verificar sala
    var salaDisponivel = await service.IsDisponivelAsync(r.SalaId, r.DataHoraInicio, r.DataHoraFim, id);
    if (!salaDisponivel)
        return Results.Conflict("Este horário já está reservado para a sala selecionada.");

    // 2. Verificar profissional
    var profissionalDisponivel = await service.IsProfissionalDisponivelAsync(r.ProfissionalId, r.DataHoraInicio, r.DataHoraFim, id);
    if (!profissionalDisponivel)
        return Results.Conflict("Este profissional já tem uma reserva nesse horário noutra sala.");

    // ✅ ATUALIZADO: Incluir a mensagem
    existing.SalaId = r.SalaId;
    existing.ProfissionalId = r.ProfissionalId;
    existing.DataHoraInicio = r.DataHoraInicio;
    existing.DataHoraFim = r.DataHoraFim;
    existing.UserId = r.UserId;
    existing.Mensagem = r.Mensagem; // ✅ ADICIONAR ESTA LINHA

    await service.UpdateAsync(existing);
    return Results.Ok($"Reserva ID {existing.Id} editada com sucesso.");
});

app.MapDelete("/api/reservas/{id:int}", async (int id, ReservaService service) =>
{
    await service.DeleteAsync(id);
    return Results.Ok($"Reserva ID {id} eliminada com sucesso.");
});



// -----------------------------
// CRUD Horários 
// -----------------------------

// GET todos os horários
app.MapGet("/api/horarios", async ([FromServices] HorarioService service) =>
{
    try
    {
        var horarios = await service.GetAllAsync();

        var dtoList = horarios.Select(h => new HorarioDto
        {
            Id = h.Id,
            HoraInicio = h.HoraInicio,
            HoraFim = h.HoraFim
        }).ToList();

        return Results.Ok(dtoList);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message + "\n" + ex.StackTrace);
    }
});


// GET horário por ID
app.MapGet("/api/horarios/{id:int}", async (int id, [FromServices] HorarioService service) =>
{
    var h = await service.GetByIdAsync(id);
    if (h == null)
        return Results.NotFound("Horário não existe.");

    var dto = new HorarioDto
    {
        Id = h.Id,
        HoraInicio = h.HoraInicio,
        HoraFim = h.HoraFim
    };

    return Results.Ok(dto);
});


// POST horário
app.MapPost("/api/horarios", async ([FromServices] HorarioService service, [FromBody] HorarioDto dto) =>
{
    if (dto.HoraFim <= dto.HoraInicio)
        return Results.BadRequest("Hora de fim deve ser posterior à de início.");

    var h = new Horario
    {
        HoraInicio = dto.HoraInicio,
        HoraFim = dto.HoraFim
    };

    await service.AddAsync(h);
    return Results.Ok($"Horário criado com sucesso ({h.HoraInicio} - {h.HoraFim}).");
});


// PUT horário
app.MapPut("/api/horarios/{id:int}", async ([FromServices] HorarioService service, int id, [FromBody] HorarioDto dto) =>
{
    var existing = await service.GetByIdAsync(id);
    if (existing == null)
        return Results.NotFound("Horário não existe.");

    if (dto.HoraFim <= dto.HoraInicio)
        return Results.BadRequest("Hora de fim deve ser posterior à de início.");

    existing.HoraInicio = dto.HoraInicio;
    existing.HoraFim = dto.HoraFim;

    await service.UpdateAsync(existing);
    return Results.Ok($"Horário ID {existing.Id} editado com sucesso.");
});


// DELETE horário
app.MapDelete("/api/horarios/{id:int}", async ([FromServices] HorarioService service, int id) =>
{
    await service.DeleteAsync(id);
    return Results.Ok($"Horário ID {id} eliminado com sucesso.");
});

// -----------------------------
// CRUD Users
// -----------------------------
app.MapGet("/api/users", async (UserService service) =>
{
    var dtos = await service.GetAllAsync();
    return Results.Ok(dtos);
});

app.MapGet("/api/users/{id:int}", async (int id, UserService service) =>
{
    var dto = await service.GetByIdAsync(id);
    return dto != null ? Results.Ok(dto) : Results.NotFound("User não existe.");
});

app.MapPost("/api/users", async (User u, UserService service, EmailService emailService) =>
{
    var validationContext = new ValidationContext(u);
    var validationResults = new List<ValidationResult>();
    if (!Validator.TryValidateObject(u, validationContext, validationResults, true))
        return Results.BadRequest(validationResults.Select(vr => vr.ErrorMessage));

    if (await service.GetByEmailAsync(u.Email) != null)
        return Results.BadRequest("Já existe um usuário com este email.");

    try
    {
        await service.AddAsync(u);

        // Gerar token para definir password
        u.PasswordResetToken = Guid.NewGuid().ToString();
        u.PasswordResetExpires = DateTime.UtcNow.AddHours(24);
        await service.UpdateAsync(u);

        // Enviar email com link
        var resetLink = $"https://tua-app/AlterarPassword?token={u.PasswordResetToken}";
        await emailService.SendEmailAsync(u.Email, "Defina a sua palavra-passe",
            $"Olá {u.Nome},<br><br>Clique no link para definir a sua password: <a href='{resetLink}'>Definir Password</a><br><br>Este link é válido por 24 horas.");

        return Results.Ok(u);
    }
    catch (DbUpdateException dbEx)
    {
        return Results.BadRequest("Erro ao criar user: " + dbEx.Message);
    }
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
// Enviar email de reset manual (se precisares chamar fora da criação)
// -----------------------------
app.MapPost("/api/users/send-password-reset", async (int userId, UserService userService, EmailService emailService) =>
{
    var user = await userService.GetEntityByIdAsync(userId);
    if (user == null) return Results.NotFound("User não existe.");

    user.PasswordResetToken = Guid.NewGuid().ToString();
    user.PasswordResetExpires = DateTime.UtcNow.AddHours(1);
    await userService.UpdateAsync(user);

    var resetLink = $"https://tua-app/AlterarPassword?token={user.PasswordResetToken}";
    await emailService.SendEmailAsync(user.Email, "Defina a sua palavra-passe",
        $"Olá {user.Nome},<br><br>Clique no link para definir a sua password: <a href='{resetLink}'>Definir Password</a><br><br>Este link é válido por 1 hora.");

    return Results.Ok($"Email enviado para {user.Email}");
});

// -----------------------------
// Login endpoint
// -----------------------------
app.MapPost("/api/auth/login", async ([FromBody] LoginDto login, UserService userService) =>
{
    var user = await userService.GetByEmailAsync(login.Email);
    if (user == null || user.PasswordHash == null || user.PasswordSalt == null)
        return Results.BadRequest("Email ou password inválidos.");

    using var hmac = new HMACSHA512(user.PasswordSalt);
    var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(login.Password));

    if (!computedHash.SequenceEqual(user.PasswordHash))
        return Results.BadRequest("Email ou password inválidos.");

    return Results.Ok(new
    {
        user.Id,
        user.Nome,
        user.Email,
        user.RoleId,
        ProfissionalId = user.Profissional?.Id
    });
});

// -----------------------------
// Definir senha (com DTO)
// -----------------------------
app.MapPost("/api/users/set-password", async (SetPasswordDto dto, UserService service) =>
{
    User? userEntity = null;

    if (!string.IsNullOrEmpty(dto.Token))
    {
        // reset via token
        userEntity = await service.GetByTokenAsync(dto.Token);
        if (userEntity == null || userEntity.PasswordResetExpires < DateTime.UtcNow)
            return Results.BadRequest("Token inválido ou expirado.");
    }
    else if (dto.UserId > 0)
    {
        // alteração interna pelo userId
        userEntity = await service.GetEntityByIdAsync(dto.UserId);
        if (userEntity == null) return Results.NotFound("User não existe.");
    }
    else
    {
        return Results.BadRequest("É necessário fornecer UserId ou Token.");
    }

    using var hmac = new HMACSHA512();
    userEntity.PasswordSalt = hmac.Key;
    userEntity.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dto.Password));

    // Reset token deixa de ser válido
    userEntity.PasswordResetToken = null;
    userEntity.PasswordResetExpires = null;

    await service.UpdateAsync(userEntity);
    return Results.Ok($"Password definida com sucesso para '{userEntity.Nome}'.");
});

// -----------------------------
// Razor Components
// -----------------------------
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

// -----------------------------
// DTO para set-password
// -----------------------------
public class SetPasswordDto
{
    public int UserId { get; set; } = 0;       // usado para alteração interna
    public string Token { get; set; } = "";    // usado para reset via email
    public string Password { get; set; } = string.Empty;
}
