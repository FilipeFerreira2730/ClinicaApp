using System.ComponentModel.DataAnnotations;

namespace ClinicaApp.DTO
{
    public class UserDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nome é obrigatório")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;

        public string Telefone { get; set; } = string.Empty;

        [Required]
        public int RoleId { get; set; }

        public string? RoleNome { get; set; }
        public ProfissionalDto? Profissional { get; set; }
    }
}
