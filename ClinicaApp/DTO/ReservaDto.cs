namespace ClinicaApp.DTO
{
    public class ReservaDto
    {
        public int Id { get; set; }
        public int SalaId { get; set; }
        public string SalaNome { get; set; }

        public int ProfissionalId { get; set; }
        public string ProfissionalNome { get; set; }

        public DateTime DataHoraInicio { get; set; }
        public DateTime DataHoraFim { get; set; }

        public int UserId { get; set; }
        public string UserNome { get; set; }
    }

}
