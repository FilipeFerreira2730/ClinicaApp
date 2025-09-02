using Microsoft.EntityFrameworkCore;
using ClinicaApp.Data;

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

        public async Task<List<Profissional>> GetAllAsync() => await _context.Profissionais.Include(p => p.User).ToListAsync();
        public async Task<Profissional> GetByIdAsync(int id) => await _context.Profissionais.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == id);
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

        public async Task<List<Reserva>> GetAllAsync() => await _context.Reservas.Include(r => r.Sala).Include(r => r.Profissional).Include(r => r.User).ToListAsync();
        public async Task<Reserva> GetByIdAsync(int id) => await _context.Reservas.Include(r => r.Sala).Include(r => r.Profissional).Include(r => r.User).FirstOrDefaultAsync(r => r.Id == id);
        public async Task AddAsync(Reserva r) { _context.Reservas.Add(r); await _context.SaveChangesAsync(); }
        public async Task UpdateAsync(Reserva r) { _context.Reservas.Update(r); await _context.SaveChangesAsync(); }
        public async Task DeleteAsync(int id) { var r = await _context.Reservas.FindAsync(id); if (r != null) { _context.Reservas.Remove(r); await _context.SaveChangesAsync(); } }

        public async Task<bool> IsDisponivelAsync(int salaId, DateTime inicio, DateTime fim)
        {
            return !await _context.Reservas.AnyAsync(r => r.SalaId == salaId && r.DataHoraInicio < fim && r.DataHoraFim > inicio);
        }
    }

    public class UserService
    {
        private readonly AppDbContext _context;
        public UserService(AppDbContext context) => _context = context;

        public async Task<List<User>> GetAllAsync() => await _context.Users.Include(u => u.Role).Include(u => u.Profissional).ToListAsync();
        public async Task<User> GetByIdAsync(int id) => await _context.Users.Include(u => u.Role).Include(u => u.Profissional).FirstOrDefaultAsync(u => u.Id == id);

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

        // Método especial: cria UserManager + Profissional
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