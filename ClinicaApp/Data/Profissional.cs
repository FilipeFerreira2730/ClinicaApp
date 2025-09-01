using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Profissional
{
    public int Id { get; set; }           
    public string Especialidade { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }
}
