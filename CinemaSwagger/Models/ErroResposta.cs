namespace CinemaSwagger.Models;

/// <summary>Mensagem explicando por que a API recusou a operação (códigos 404 e 409).</summary>
/// <param name="Mensagem">Texto do motivo, por exemplo: "Só restam 3 lugar(es) livre(s) nesta sessão."</param>
public record ErroResposta(string Mensagem);
