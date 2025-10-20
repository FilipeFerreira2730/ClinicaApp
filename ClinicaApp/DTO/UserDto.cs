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

        [Required(ErrorMessage = "Telefone é obrigatório")]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "O telemóvel deve ter 9 dígitos numéricos")]
        public string Telefone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role é obrigatório")]
        public int RoleId { get; set; }

        public string? RoleNome { get; set; }
        public ProfissionalDto? Profissional { get; set; }
    }
}
