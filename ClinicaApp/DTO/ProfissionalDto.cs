namespace ClinicaApp.DTO
{
    public class ProfissionalDto
    {
        public int Id { get; set; }
        public string Especialidade { get; set; }
        public int UserId { get; set; }
        public string UserNome { get; set; } // opcional, para exibir nome do user
    }
}
