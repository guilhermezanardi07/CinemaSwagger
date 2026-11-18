namespace CinemaSwagger.Models;

/// <summary>Informações retornadas pela API sobre uma reserva, com dados da sessão e do filme.</summary>
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
