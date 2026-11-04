namespace CinemaSwagger.Models;

/// <summary>Informações retornadas pela API sobre uma sessão e seu filme.</summary>
public record SessaoResposta(
    int Id,
    int FilmeId,
    string TituloFilme,
    string Sala,
    DateTime DataHora,
    decimal Preco,
    int Capacidade
);
