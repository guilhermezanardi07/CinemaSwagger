namespace CinemaSwagger.Models;

/// <summary>Reserva de um assento em uma sessão.</summary>
public class Reserva
{
    public int Id { get; set; }
    public int SessaoId { get; set; }
    public Sessao Sessao { get; set; } = null!;
    public string Assento { get; set; } = "";
    public DateTime CriadaEm { get; set; }
}
