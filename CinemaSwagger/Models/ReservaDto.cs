using System.ComponentModel.DataAnnotations;

namespace CinemaSwagger.Models;

public class ReservaDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Informe uma sessão válida.")]
    public int SessaoId { get; set; }

    /// <example>12</example>
    [Required, StringLength(5, MinimumLength = 2)]
    [RegularExpression(@"^[0-9]{1,4}$", ErrorMessage = "Informe o número do assento, de 1 a 1000.")]
    public string Assento { get; set; } = "";
}
