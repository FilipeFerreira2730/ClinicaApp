using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicaApp.Data
{
    public class Horario
    {
        public int Id { get; set; }

        [Required]
        public TimeSpan HoraInicio { get; set; }

        [Required]
        public TimeSpan HoraFim { get; set; }
    }
}
