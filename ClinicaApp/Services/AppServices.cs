using Microsoft.EntityFrameworkCore;
using ClinicaApp.Data;
using ClinicaApp.DTO;

namespace ClinicaApp.Services
{
    public class SalaService
    {
        private readonly AppDbContext _context;
        public SalaService(AppDbContext context) => _context = context;

        public async Task<List<Sala>> GetAllAsync() => await _context.Salas.ToListAsync();
        public async Task<Sala> GetByIdAsync(int id) => await _context.Salas.FindAsync(id);
        public async Task AddAsync(Sala sala) { _context.Salas.Add(sala); await _context.SaveChangesAsync(); }
        public async Task UpdateAsync(Sala sala) { _context.Salas.Update(sala); await _context.SaveChangesAsync(); }
        public async Task DeleteAsync(int id)
        {
            var s = await _context.Salas.FindAsync(id);
            if (s != null) { _context.Salas.Remove(s); await _context.SaveChangesAsync(); }
        }
    }

    public class ProfissionalService
    {
        private readonly AppDbContext _context;
        public ProfissionalService(AppDbContext context) => _context = context;

        // DTOs para leitura
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

        public async Task<ProfissionalDto> GetByIdAsync(int id)
        {
            var p = await _context.Profissionais.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == id);
            if (p == null) return null;

            return new ProfissionalDto
            {
                Id = p.Id,
                Especialidade = p.Especialidade,
                UserId = p.UserId,
                UserNome = p.User != null ? p.User.Nome : null
            };
        }

        // Entidade real, para criar/editar
        public async Task<Profissional> GetEntityByIdAsync(int id)
        {
            return await _context.Profissionais.FindAsync(id);
        }

        public async Task AddAsync(Profissional p) { _context.Profissionais.Add(p); await _context.SaveChangesAsync(); }
        public async Task UpdateAsync(Profissional p) { _context.Profissionais.Update(p); await _context.SaveChangesAsync(); }
        public async Task DeleteAsync(int id)
        {
            var p = await _context.Profissionais.FindAsync(id);
            if (p != null) { _context.Profissionais.Remove(p); await _context.SaveChangesAsync(); }
        }
    }

    public class ReservaService
    {
        private readonly AppDbContext _context;
        public ReservaService(AppDbContext context) => _context = context;

        // DTO para evitar ciclos
        public async Task<List<ReservaDto>> GetAllAsync()
        {
            return await _context.Reservas
                .Include(r => r.Sala)
                .Include(r => r.Profissional)
                    .ThenInclude(p => p.User)
                .Include(r => r.User)
                .Select(r => new ReservaDto
                {
                    Id = r.Id,
                    SalaId = r.SalaId,
                    SalaNome = r.Sala.Nome,
                    ProfissionalId = r.ProfissionalId,
                    ProfissionalNome = r.Profissional.User != null ? r.Profissional.User.Nome : null,
                    DataHoraInicio = r.DataHoraInicio,
                    DataHoraFim = r.DataHoraFim,
                    UserId = r.UserId,
                    UserNome = r.User != null ? r.User.Nome : null
                })
                .ToListAsync();
        }

        public async Task<ReservaDto> GetByIdAsync(int id)
        {
            var r = await _context.Reservas
                .Include(r => r.Sala)
                .Include(r => r.Profissional)
                    .ThenInclude(p => p.User)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (r == null) return null;

            return new ReservaDto
            {
                Id = r.Id,
                SalaId = r.SalaId,
                SalaNome = r.Sala.Nome,
                ProfissionalId = r.ProfissionalId,
                ProfissionalNome = r.Profissional.User != null ? r.Profissional.User.Nome : null,
                DataHoraInicio = r.DataHoraInicio,
                DataHoraFim = r.DataHoraFim,
                UserId = r.UserId,
                UserNome = r.User != null ? r.User.Nome : null
            };
        }

        // ✅ Entidade real (para update/delete)
        public async Task<Reserva> GetEntityByIdAsync(int id)
        {
            return await _context.Reservas.FindAsync(id);
        }

        public async Task AddAsync(Reserva r) { _context.Reservas.Add(r); await _context.SaveChangesAsync(); }
        public async Task UpdateAsync(Reserva r) { _context.Reservas.Update(r); await _context.SaveChangesAsync(); }
        public async Task DeleteAsync(int id)
        {
            var r = await _context.Reservas.FindAsync(id);
            if (r != null) { _context.Reservas.Remove(r); await _context.SaveChangesAsync(); }
        }

        public async Task<bool> IsDisponivelAsync(int salaId, DateTime inicio, DateTime fim)
        {
            return !await _context.Reservas.AnyAsync(r => r.SalaId == salaId && r.DataHoraInicio < fim && r.DataHoraFim > inicio);
        }
    }

    public class UserService
    {
        private readonly AppDbContext _context;
        public UserService(AppDbContext context) => _context = context;

        // DTOs para leitura
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

        public async Task<UserDto> GetByIdAsync(int id)
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
                RoleNome = u.Role != null ? u.Role.Nome : null,
                Profissional = u.Profissional != null ? new ProfissionalDto
                {
                    Id = u.Profissional.Id,
                    Especialidade = u.Profissional.Especialidade,
                    UserId = u.Id,
                    UserNome = u.Nome
                } : null
            };
        }

        // Entidade real para criação/edição (incluindo password)
        public async Task<User> GetEntityByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task AddAsync(User user) { _context.Users.Add(user); await _context.SaveChangesAsync(); }
        public async Task UpdateAsync(User user) { _context.Users.Update(user); await _context.SaveChangesAsync(); }
        public async Task DeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null) { _context.Users.Remove(user); await _context.SaveChangesAsync(); }
        }

        // Criar UserManager + Profissional
        public async Task<User> CreateUserManagerAsync(string nome, string email, string telefone, string especialidade, byte[] passwordHash, byte[] passwordSalt)
        {
            var user = new User
            {
                Nome = nome,
                Email = email,
                Telefone = telefone,
                RoleId = 2, // UserManager
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var profissional = new Profissional
            {
                UserId = user.Id,
                Especialidade = especialidade
            };

            _context.Profissionais.Add(profissional);
            await _context.SaveChangesAsync();

            return user;
        }
    }
}
