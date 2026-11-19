namespace CinemaSwagger.Models;

/// <summary>Situação de lugares de uma sessão: capacidade, reservados e livres.</summary>
/// <param name="SessaoId">Id da sessão.</param>
/// <param name="FilmeId">Id do filme da sessão.</param>
/// <param name="TituloFilme">Título do filme.</param>
/// <param name="Sala">Sala da sessão.</param>
/// <param name="DataHora">Início da sessão, em UTC.</param>
/// <param name="Preco">Preço de um ingresso, em reais.</param>
/// <param name="Capacidade">Total de lugares da sessão.</param>
/// <param name="LugaresReservados">Lugares ocupados por reservas Confirmadas.</param>
/// <param name="LugaresLivres">Capacidade menos os lugares reservados.</param>
/// <param name="Esgotada">true quando não há mais lugares livres.</param>
/// <param name="JaComecou">true quando a sessão já começou (não aceita mais reservas).</param>
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