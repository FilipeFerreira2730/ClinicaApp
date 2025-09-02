using Microsoft.EntityFrameworkCore;

namespace ClinicaApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Sala> Salas { get; set; }
        public DbSet<Profissional> Profissionais { get; set; }
        public DbSet<Reserva> Reservas { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Sala
            modelBuilder.Entity<Sala>(e =>
            {
                e.Property(p => p.Nome).IsRequired().HasMaxLength(100);
                e.HasIndex(p => p.Nome).IsUnique();
            });

            // Profissional
            modelBuilder.Entity<Profissional>(e =>
            {
                e.Property(p => p.Especialidade).HasMaxLength(100);

                // Relação 1:1 com User
                e.HasOne(p => p.User)
                 .WithOne(u => u.Profissional)
                 .HasForeignKey<Profissional>(p => p.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // Reserva
            modelBuilder.Entity<Reserva>(e =>
            {
                e.Property(p => p.DataHoraInicio).IsRequired();
                e.Property(p => p.DataHoraFim).IsRequired();

                e.HasOne(r => r.Sala)
                 .WithMany()
                 .HasForeignKey(r => r.SalaId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(r => r.Profissional)
                 .WithMany()
                 .HasForeignKey(r => r.ProfissionalId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(r => r.User)
                 .WithMany()
                 .HasForeignKey(r => r.UserId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasIndex(r => new { r.SalaId, r.DataHoraInicio, r.DataHoraFim });
            });

            // Role
            modelBuilder.Entity<Role>(e =>
            {
                e.Property(p => p.Nome).IsRequired().HasMaxLength(50);
                e.HasIndex(p => p.Nome).IsUnique();
            });

            // User
            modelBuilder.Entity<User>(e =>
            {
                e.Property(p => p.Nome).IsRequired().HasMaxLength(150);
                e.Property(p => p.Email).IsRequired().HasMaxLength(200);
                e.Property(p => p.Telefone).HasMaxLength(50);

                e.HasOne(u => u.Role)
                 .WithMany()
                 .HasForeignKey(u => u.RoleId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasIndex(u => u.Email).IsUnique();
            });
        }
    }
}
