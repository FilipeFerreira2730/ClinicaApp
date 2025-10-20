using ClinicaApp.Data;

public class User
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Email { get; set; }

    // Nullable => permite NULL até o utilizador definir a password
    public byte[]? PasswordHash { get; set; }
    public byte[]? PasswordSalt { get; set; }

    public int RoleId { get; set; }
    public Role Role { get; set; }

    public string Telefone { get; set; }  // agora pertence ao User

    // Navegação para Profissional, se for RoleId=2
    public Profissional? Profissional { get; set; }

    // 🔑 Campos para reset de password
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetExpires { get; set; }
}
