namespace CinemaSwagger.Models;

/// <summary>Reserva devolvida pela API, já com os dados da sessão e do filme.</summary>
/// <param name="Id">Id da reserva.</param>
/// <param name="SessaoId">Id da sessão reservada.</param>
/// <param name="TituloFilme">Título do filme da sessão.</param>
/// <param name="Sala">Sala onde a sessão acontece.</param>
/// <param name="DataHoraSessao">Início da sessão, em UTC.</param>
/// <param name="NomeCliente">Nome de quem reservou.</param>
/// <param name="Email">E-mail do cliente, sempre em minúsculas.</param>
/// <param name="QuantidadeLugares">Quantos lugares foram reservados.</param>
/// <param name="ValorTotal">Preço da sessão x quantidade de lugares, em reais.</param>
/// <param name="Status">Situação da reserva: Confirmada ou Cancelada.</param>
/// <param name="DataReserva">Momento em que a reserva foi feita, em UTC.</param>
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
