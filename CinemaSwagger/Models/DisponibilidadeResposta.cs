namespace CinemaSwagger.Models;

public record DisponibilidadeResposta(
    int SessaoId,
    int Capacidade,
    int LugaresOcupados,
    int LugaresDisponiveis,
    IReadOnlyList<string> AssentosOcupados
);
