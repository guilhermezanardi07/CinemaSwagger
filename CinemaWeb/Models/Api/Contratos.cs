using System.ComponentModel.DataAnnotations;

namespace CinemaWeb.Models.Api;

// Classes que espelham o que a API CinemaSwagger envia e recebe.

/// <summary>Filme devolvido pela API (GET /api/Filmes).</summary>
public class FilmeResposta
{
    public int Id { get; set; }
    public string Titulo { get; set; } = "";
    public string Genero { get; set; } = "";
    public int DuracaoMinutos { get; set; }
    public string Classificacao { get; set; } = "";
    public string? Sinopse { get; set; }
    public DateOnly DataLancamento { get; set; }
    public bool EmCartaz { get; set; }
}

/// <summary>Corpo de POST/PUT /api/Filmes. Também serve de modelo do formulário.</summary>
public class FilmeDto
{
    [Display(Name = "Título")]
    [Required(ErrorMessage = "Informe o título.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "O título deve ter até 100 caracteres.")]
    public string Titulo { get; set; } = "";

    [Display(Name = "Gênero")]
    [Required(ErrorMessage = "Informe o gênero.")]
    [StringLength(50, ErrorMessage = "O gênero deve ter até 50 caracteres.")]
    public string Genero { get; set; } = "";

    [Display(Name = "Duração (minutos)")]
    [Range(1, 600, ErrorMessage = "A duração deve ser de 1 a 600 minutos.")]
    public int DuracaoMinutos { get; set; } = 90;

    [Display(Name = "Classificação")]
    [Required(ErrorMessage = "Informe a classificação.")]
    [StringLength(20, ErrorMessage = "A classificação deve ter até 20 caracteres.")]
    public string Classificacao { get; set; } = "Livre";

    [Display(Name = "Sinopse")]
    [StringLength(500, ErrorMessage = "A sinopse deve ter até 500 caracteres.")]
    public string? Sinopse { get; set; }

    [Display(Name = "Data de lançamento")]
    public DateOnly DataLancamento { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Display(Name = "Em cartaz")]
    public bool EmCartaz { get; set; } = true;
}

public record SessaoResposta(
    int Id,
    int FilmeId,
    string TituloFilme,
    string Sala,
    DateTime DataHora,
    decimal Preco,
    int Capacidade
);

/// <summary>Corpo de POST/PUT /api/Sessoes. DataHora precisa ir em UTC.</summary>
public class SessaoDto
{
    public int FilmeId { get; set; }
    public string Sala { get; set; } = "";
    public DateTime DataHora { get; set; }
    public decimal Preco { get; set; }
    public int Capacidade { get; set; }
}

public record DisponibilidadeResposta(
    int SessaoId,
    int FilmeId,
    string TituloFilme,
    string Sala,
    DateTime DataHora,
    decimal Preco,
    int Capacidade,
    int LugaresReservados,
    int LugaresLivres,
    bool Esgotada,
    bool JaComecou
);

public record ReservaResposta(
    int Id,
    int SessaoId,
    string TituloFilme,
    string Sala,
    DateTime DataHoraSessao,
    string NomeCliente,
    string Email,
    int QuantidadeLugares,
    decimal ValorTotal,
    string Status,
    DateTime DataReserva
);

/// <summary>Corpo de POST /api/Reservas.</summary>
public class ReservaDto
{
    public int SessaoId { get; set; }
    public string NomeCliente { get; set; } = "";
    public string Email { get; set; } = "";
    public int QuantidadeLugares { get; set; }
}
