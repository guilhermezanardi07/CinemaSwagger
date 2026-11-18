namespace CinemaSwagger.Models;

/// <summary>Reserva de lugares feita por um cliente em uma sessão.</summary>
public class Reserva
{
    public int Id { get; set; }

    public int SessaoId { get; set; }
    public Sessao Sessao { get; set; } = null!;

    public string NomeCliente { get; set; } = "";
    public string Email { get; set; } = "";

    /// <summary>Quantidade de lugares reservados.</summary>
    public int QuantidadeLugares { get; set; }

    /// <summary>Preço da sessão multiplicado pela quantidade de lugares, em reais.</summary>
    public decimal ValorTotal { get; set; }

    /// <summary>Situação da reserva: Confirmada ou Cancelada.</summary>
    public string Status { get; set; } = StatusReserva.Confirmada;

    /// <summary>Data e hora em que a reserva foi feita (UTC).</summary>
    public DateTime DataReserva { get; set; }
}

/// <summary>Valores possíveis para a situação de uma reserva.</summary>
public static class StatusReserva
{
    public const string Confirmada = "Confirmada";
    public const string Cancelada = "Cancelada";

    public static readonly string[] Todos = { Confirmada, Cancelada };
}
