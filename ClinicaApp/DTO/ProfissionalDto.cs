using System.ComponentModel.DataAnnotations;

namespace ClinicaApp.DTO
{
    public class ProfissionalDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        [Required(ErrorMessage = "Especialidade é obrigatória")]
        public string Especialidade { get; set; } = string.Empty;
        public string UserNome { get; set; } // opcional, para exibir nome do user
    }
}
