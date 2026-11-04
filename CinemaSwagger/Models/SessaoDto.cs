using System.ComponentModel.DataAnnotations;

namespace CinemaSwagger.Models;

/// <summary>Dados necessários para cadastrar ou atualizar uma sessão.</summary>
public class SessaoDto
{
    /// <example>1</example>
    [Range(1, int.MaxValue, ErrorMessage = "Informe um FilmeId válido.")]
    public int FilmeId { get; set; }

    /// <example>Sala 1</example>
    [Required(ErrorMessage = "A sala é obrigatória.")]
    [StringLength(30, MinimumLength = 1)]
    public string Sala { get; set; } = "";

    /// <summary>Horário de início em UTC. Envie com Z, por exemplo: 2026-12-15T22:30:00Z.</summary>
    /// <example>2026-12-15T22:30:00Z</example>
    public DateTime DataHora { get; set; }

    /// <summary>Preço em reais (ex.: 25.50 significa R$ 25,50).</summary>
    /// <example>25.50</example>
    [Range(typeof(decimal), "0", "10000", ErrorMessage = "O preço deve estar entre 0 e 10000 reais.")]
    public decimal Preco { get; set; }

    /// <example>80</example>
    [Range(1, 1000, ErrorMessage = "A capacidade deve ser de 1 a 1000 lugares.")]
    public int Capacidade { get; set; }
}
