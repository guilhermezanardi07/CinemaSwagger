namespace CinemaSwagger.Models;

public class Filme
{
    public int Id { get; set; }
    public string Titulo { get; set; } = "";
    public string Genero { get; set; } = "";
    public int DuracaoMinutos { get; set; }
    public string Classificacao { get; set; } = "Livre";
    public string? Sinopse { get; set; }
    public DateOnly DataLancamento { get; set; }
    public bool EmCartaz { get; set; } = true;
}