namespace ClinicaApp.DTO
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }
        public int RoleId { get; set; }
        public string RoleNome { get; set; } // opcional, para mostrar o nome da role

        // Nullable, pois nem todos os users têm Profissional
        public ProfissionalDto? Profissional { get; set; }
    }
}
