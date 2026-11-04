namespace CinemaSwagger.Models;

/// <summary>Representa a exibição de um filme em uma sala do cinema.</summary>
public class Sessao
{
    public int Id { get; set; }

    public int FilmeId { get; set; }
    public Filme Filme { get; set; } = null!;

    public string Sala { get; set; } = "";

    /// <summary>Data e horário de início da exibição (UTC).</summary>
    public DateTime DataHora { get; set; }

    /// <summary>Preço de um ingresso, em reais.</summary>
    public decimal Preco { get; set; }

    /// <summary>Quantidade total de lugares desta sessão.</summary>
    public int Capacidade { get; set; }
}
