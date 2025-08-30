using System.Collections.Generic;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;

namespace ClinicaApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Sala> Salas { get; set; }
        public DbSet<Profissional> Profissionais { get; set; }
        public DbSet<Reserva> Reservas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Sala
            modelBuilder.Entity<Sala>(e =>
            {
                e.Property(p => p.Nome).IsRequired().HasMaxLength(100);
                e.HasIndex(p => p.Nome).IsUnique(); // evita nomes de sala duplicados
            });

            // Profissional
            modelBuilder.Entity<Profissional>(e =>
            {
                e.Property(p => p.Nome).IsRequired().HasMaxLength(150);
                e.Property(p => p.Especialidade).HasMaxLength(100);
                e.Property(p => p.Email).HasMaxLength(200);
                e.Property(p => p.Telefone).HasMaxLength(50);
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

                // índice útil para pesquisar disponibilidade rapidamente
                e.HasIndex(r => new { r.SalaId, r.DataHoraInicio, r.DataHoraFim });
            });
        }
    }
}
