using System;

namespace ClinicaApp.Data
{
    public class Reserva
    {
        public int Id { get; set; }

        public int SalaId { get; set; }
        public Sala Sala { get; set; }

        public int ProfissionalId { get; set; }
        public Profissional Profissional { get; set; }

        public DateTime DataHoraInicio { get; set; }
        public DateTime DataHoraFim { get; set; }
    }
}
