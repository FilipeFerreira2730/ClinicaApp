using Microsoft.EntityFrameworkCore;
using ClinicaApp.Data;
using ClinicaApp.DTO;
using System.Net;
using System.Net.Mail;

namespace ClinicaApp
{
    // ==============================
    // SALAS
    // ==============================
    public class SalaService
    {
        private readonly AppDbContext _context;
        public SalaService(AppDbContext context) => _context = context;

        public async Task<List<Sala>> GetAllAsync() => await _context.Salas.ToListAsync();
        public async Task<Sala?> GetByIdAsync(int id) => await _context.Salas.FindAsync(id);

        public async Task AddAsync(Sala sala)
        {
            _context.Salas.Add(sala);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Sala sala)
        {
            _context.Salas.Update(sala);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var s = await _context.Salas.FindAsync(id);
            if (s != null)
            {
                _context.Salas.Remove(s);
                await _context.SaveChangesAsync();
            }
        }
    }

    // ==============================
    // PROFISSIONAIS
    // ==============================
    public class ProfissionalService
    {
        private readonly AppDbContext _context;
        public ProfissionalService(AppDbContext context) => _context = context;

        public async Task<List<ProfissionalDto>> GetAllAsync()
        {
            return await _context.Profissionais
                .Include(p => p.User)
                .Select(p => new ProfissionalDto
                {
                    Id = p.Id,
                    Especialidade = p.Especialidade,
                    UserId = p.UserId,
                    UserNome = p.User != null ? p.User.Nome : null
                })
                .ToListAsync();
        }

        public async Task<ProfissionalDto?> GetByIdAsync(int id)
        {
            var p = await _context.Profissionais
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (p == null) return null;

            return new ProfissionalDto
            {
                Id = p.Id,
                Especialidade = p.Especialidade,
                UserId = p.UserId,
                UserNome = p.User != null ? p.User.Nome : null
            };
        }

        public async Task<Profissional?> GetEntityByIdAsync(int id) => await _context.Profissionais.FindAsync(id);

        public async Task AddAsync(Profissional p)
        {
            _context.Profissionais.Add(p);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Profissional p)
        {
            _context.Profissionais.Update(p);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var p = await _context.Profissionais.FindAsync(id);
            if (p != null)
            {
                _context.Profissionais.Remove(p);
                await _context.SaveChangesAsync();
            }
        }
    }

    // ==============================
    // STATE USER
    // ==============================
    public class UserStateService
    {
        public UserDto? UsuarioLogado { get; private set; }
        public event Action? OnChange;

        public void SetUsuario(UserDto usuario)
        {
            UsuarioLogado = usuario;
            OnChange?.Invoke();
        }

        public void ClearUsuario()
        {
            UsuarioLogado = null;
            OnChange?.Invoke();
        }
    }


    // ==============================
    // HORÁRIOS
    // ==============================
    public class HorarioService
    {
        private readonly AppDbContext _context;
        public HorarioService(AppDbContext context) => _context = context;

        public async Task<List<Horario>> GetAllAsync() => await _context.Horarios.ToListAsync();

        public async Task<Horario?> GetByIdAsync(int id) => await _context.Horarios.FindAsync(id);

        // 🔹 Adiciona este método — é o que falta
        public async Task<Horario?> GetEntityByIdAsync(int id)
        {
            return await _context.Horarios.FindAsync(id);
        }

        public async Task AddAsync(Horario horario)
        {
            _context.Horarios.Add(horario);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Horario horario)
        {
            _context.Horarios.Update(horario);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var h = await _context.Horarios.FindAsync(id);
            if (h != null)
            {
                _context.Horarios.Remove(h);
                await _context.SaveChangesAsync();
            }
        }
    }


    // ==============================
    // RESERVAS
    // ==============================
    public class ReservaService
    {
        private readonly AppDbContext _context;
        public ReservaService(AppDbContext context) => _context = context;

        public async Task<List<ReservaDto>> GetAllAsync()
        {
            return await _context.Reservas
                .Include(r => r.Sala)
                .Include(r => r.Profissional).ThenInclude(p => p.User)
                .Include(r => r.User)
                .Select(r => new ReservaDto
                {
                    Id = r.Id,
                    SalaId = r.SalaId,
                    SalaNome = r.Sala != null ? r.Sala.Nome : null,
                    ProfissionalId = r.ProfissionalId,
                    ProfissionalNome = r.Profissional != null && r.Profissional.User != null ? r.Profissional.User.Nome : null,
                    DataHoraInicio = r.DataHoraInicio,
                    DataHoraFim = r.DataHoraFim,
                    UserId = r.UserId,
                    UserNome = r.User != null ? r.User.Nome : null,
                    Mensagem = r.Mensagem // ✅ Adicionado
                })
                .ToListAsync();
        }

        public async Task<ReservaDto?> GetByIdAsync(int id)
        {
            var r = await _context.Reservas
                .Include(r => r.Sala)
                .Include(r => r.Profissional).ThenInclude(p => p.User)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (r == null) return null;

            return new ReservaDto
            {
                Id = r.Id,
                SalaId = r.SalaId,
                SalaNome = r.Sala?.Nome,
                ProfissionalId = r.ProfissionalId,
                ProfissionalNome = r.Profissional?.User?.Nome,
                DataHoraInicio = r.DataHoraInicio,
                DataHoraFim = r.DataHoraFim,
                UserId = r.UserId,
                UserNome = r.User?.Nome,
                Mensagem = r.Mensagem // ✅ Adicionado
            };
        }

        // Métodos auxiliares para verificar disponibilidade
        public async Task<bool> IsDisponivelAsync(int salaId, DateTime inicio, DateTime fim, int? ignoreReservaId = null)
        {
            return !await _context.Reservas
                .AnyAsync(r => r.SalaId == salaId
                            && r.DataHoraInicio < fim
                            && r.DataHoraFim > inicio
                            && (!ignoreReservaId.HasValue || r.Id != ignoreReservaId.Value));
        }

        public async Task<bool> IsProfissionalDisponivelAsync(int profissionalId, DateTime inicio, DateTime fim, int? ignoreReservaId = null)
        {
            return !await _context.Reservas
                .AnyAsync(r => r.ProfissionalId == profissionalId
                            && r.DataHoraInicio < fim
                            && r.DataHoraFim > inicio
                            && (!ignoreReservaId.HasValue || r.Id != ignoreReservaId.Value));
        }

        // ✅ Novo método para criar reserva a partir do DTO
        public async Task<bool> CriarReservaAsync(ReservaDto reservaDto)
        {
            try
            {
                var reserva = new Reserva
                {
                    SalaId = reservaDto.SalaId,
                    ProfissionalId = reservaDto.ProfissionalId,
                    DataHoraInicio = reservaDto.DataHoraInicio,
                    DataHoraFim = reservaDto.DataHoraFim,
                    UserId = reservaDto.UserId,
                    Mensagem = reservaDto.Mensagem // ✅ Inclui a mensagem
                };

                _context.Reservas.Add(reserva);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                // Log do erro
                Console.WriteLine($"Erro ao criar reserva: {ex.Message}");
                return false;
            }
        }

        // ✅ Método para atualizar reserva
        public async Task<bool> AtualizarReservaAsync(ReservaDto reservaDto)
        {
            try
            {
                var reserva = await _context.Reservas.FindAsync(reservaDto.Id);
                if (reserva == null) return false;

                reserva.SalaId = reservaDto.SalaId;
                reserva.ProfissionalId = reservaDto.ProfissionalId;
                reserva.DataHoraInicio = reservaDto.DataHoraInicio;
                reserva.DataHoraFim = reservaDto.DataHoraFim;
                reserva.Mensagem = reservaDto.Mensagem; // ✅ Atualiza a mensagem

                _context.Reservas.Update(reserva);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao atualizar reserva: {ex.Message}");
                return false;
            }
        }

        // Mantenha os métodos existentes AddAsync, UpdateAsync, DeleteAsync, GetEntityByIdAsync
        public async Task<Reserva?> GetEntityByIdAsync(int id) => await _context.Reservas.FindAsync(id);

        public async Task AddAsync(Reserva r)
        {
            _context.Reservas.Add(r);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Reserva r)
        {
            _context.Reservas.Update(r);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var r = await _context.Reservas.FindAsync(id);
            if (r != null)
            {
                _context.Reservas.Remove(r);
                await _context.SaveChangesAsync();
            }
        }
    }

    // ==============================
    // USERS
    // ==============================
    public class UserService
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;

        public UserService(AppDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<List<UserDto>> GetAllAsync()
        {
            return await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Profissional)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Nome = u.Nome,
                    Email = u.Email,
                    Telefone = u.Telefone,
                    RoleId = u.RoleId,
                    RoleNome = u.Role != null ? u.Role.Nome : null,
                    Profissional = u.Profissional != null ? new ProfissionalDto
                    {
                        Id = u.Profissional.Id,
                        Especialidade = u.Profissional.Especialidade,
                        UserId = u.Id,
                        UserNome = u.Nome
                    } : null
                })
                .ToListAsync();
        }

        public async Task<UserDto?> GetByIdAsync(int id)
        {
            var u = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Profissional)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (u == null) return null;

            return new UserDto
            {
                Id = u.Id,
                Nome = u.Nome,
                Email = u.Email,
                Telefone = u.Telefone,
                RoleId = u.RoleId,
                RoleNome = u.Role?.Nome,
                Profissional = u.Profissional != null ? new ProfissionalDto
                {
                    Id = u.Profissional.Id,
                    Especialidade = u.Profissional.Especialidade,
                    UserId = u.Id,
                    UserNome = u.Nome
                } : null
            };
        }

        public async Task<User?> GetEntityByIdAsync(int id) => await _context.Users.FindAsync(id);
        public async Task<User?> GetByEmailAsync(string email) => await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        public async Task<User?> GetByTokenAsync(string token) => await _context.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == token);

        public async Task AddAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }

        // ==============================
        // RESET DE PASSWORD
        // ==============================
        public async Task<string> GeneratePasswordResetTokenAsync(User user)
        {
            user.PasswordResetToken = Guid.NewGuid().ToString();
            user.PasswordResetExpires = DateTime.UtcNow.AddHours(1);
            await UpdateAsync(user);

            // Enviar email de reset
            var resetLink = $"https://tua-app/AlterarPassword?token={user.PasswordResetToken}";
            await _emailService.SendEmailAsync(user.Email, "Defina a sua palavra-passe",
                $"Olá {user.Nome},<br><br>Clique no link para definir a sua password: <a href='{resetLink}'>Definir Password</a><br><br>Este link é válido por 1 hora.");

            return user.PasswordResetToken;
        }
    }

    // ==============================
    // EMAIL SERVICE
    // ==============================
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var from = _config["Email:From"];
            var smtpServer = _config["Email:Smtp:Server"];
            var smtpPort = int.Parse(_config["Email:Smtp:Port"]);
            var smtpUser = _config["Email:Smtp:User"];
            var smtpPass = _config["Email:Smtp:Pass"];

            using var client = new SmtpClient(smtpServer, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(from),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(to);

            await client.SendMailAsync(mailMessage);
        }
    }
}
