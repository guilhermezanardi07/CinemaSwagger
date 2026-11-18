using System.ComponentModel.DataAnnotations;

namespace CinemaSwagger.Models;

/// <summary>Dados para criar ou alterar uma reserva.</summary>
public class ReservaDto
{
    /// <summary>Id da sessão que será reservada. Consulte os Ids em GET /api/Sessoes.</summary>
    /// <example>1</example>
    [Range(1, int.MaxValue, ErrorMessage = "Informe um SessaoId válido.")]
    public int SessaoId { get; set; }

    /// <summary>Nome de quem está reservando (de 2 a 100 caracteres).</summary>
    /// <example>Ana Souza</example>
    [Required(ErrorMessage = "O nome do cliente é obrigatório.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter de 2 a 100 caracteres.")]
    public string NomeCliente { get; set; } = "";

    /// <summary>E-mail do cliente. É guardado em minúsculas.</summary>
    /// <example>ana.souza@email.com</example>
    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [StringLength(150)]
    public string Email { get; set; } = "";

    /// <summary>Quantos lugares reservar, de 1 a 10 por reserva.</summary>
    /// <example>2</example>
    [Range(1, 10, ErrorMessage = "Reserve de 1 a 10 lugares por vez.")]
    public int QuantidadeLugares { get; set; }
}
