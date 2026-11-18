using CinemaSwagger.Data;
using CinemaSwagger.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaSwagger.Controllers;

[ApiController]
[Route("api/sessoes/{sessaoId:int}/disponibilidade")]
[Produces("application/json")]
public class DisponibilidadeController : ControllerBase
{
    private readonly AppDbContext _db;
    public DisponibilidadeController(AppDbContext db) => _db = db;

    /// <summary>Consulta lugares disponíveis e assentos já reservados para uma sessão.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(DisponibilidadeResposta), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DisponibilidadeResposta>> Consultar(int sessaoId)
    {
        var sessao = await _db.Sessoes.AsNoTracking()
            .Where(s => s.Id == sessaoId)
            .Select(s => new { s.Id, s.Capacidade })
            .FirstOrDefaultAsync();
        if (sessao is null) return NotFound(new { mensagem = "Sessão não encontrada." });

        var ocupados = await _db.Reservas.AsNoTracking()
            .Where(r => r.SessaoId == sessaoId)
            .OrderBy(r => r.Assento)
            .Select(r => r.Assento)
            .ToListAsync();

        return Ok(new DisponibilidadeResposta(sessao.Id, sessao.Capacidade,
            ocupados.Count, sessao.Capacidade - ocupados.Count, ocupados));
    }
}
