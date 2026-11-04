using System.ComponentModel.DataAnnotations;

namespace CinemaSwagger.Models;

/// <summary>Dados para cadastrar ou atualizar um filme.</summary>
public class FilmeDto
{
    /// <example>Interestelar</example>
    [Required, StringLength(100, MinimumLength = 1)]
    public string Titulo { get; set; } = "";

    /// <example>Ficção científica</example>
    [Required, StringLength(50)]
    public string Genero { get; set; } = "";

    /// <example>169</example>
    [Range(1, 600)]
    public int DuracaoMinutos { get; set; }

    /// <example>12 anos</example>
    [Required, StringLength(20)]
    public string Classificacao { get; set; } = "Livre";

    /// <example>Exploradores viajam por um buraco de minhoca.</example>
    [StringLength(500)]
    public string? Sinopse { get; set; }

    /// <example>2014-11-06</example>
    public DateOnly DataLancamento { get; set; }

    /// <example>true</example>
    public bool EmCartaz { get; set; } = true;
}