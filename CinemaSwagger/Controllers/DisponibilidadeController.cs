using CinemaSwagger.Data;
using CinemaSwagger.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaSwagger.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DisponibilidadeController : ControllerBase
{
    private readonly AppDbContext _db;

    public DisponibilidadeController(AppDbContext db) => _db = db;

    /// <summary>Lista a disponibilidade de lugares das sessões.</summary>
    /// <remarks>
    /// Calculada em tempo real: capacidade da sessão menos os lugares das reservas Confirmadas.
    /// Reservas canceladas não ocupam lugares. Os filtros podem ser combinados.
    /// </remarks>
    /// <param name="filmeId">Filtra pelas sessões de um filme (consulte os Ids em GET /api/Filmes).</param>
    /// <param name="apenasComLugares">true = somente sessões com pelo menos 1 lugar livre.</param>
    /// <param name="apenasFuturas">true = somente sessões que ainda não começaram.</param>
    /// <response code="200">Lista de disponibilidade, da sessão mais próxima para a mais distante (pode estar vazia).</response>
    /// <response code="400">filmeId menor ou igual a zero.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<DisponibilidadeResposta>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<DisponibilidadeResposta>>> Listar(
        [FromQuery] int? filmeId,
        [FromQuery] bool? apenasComLugares,
        [FromQuery] bool? apenasFuturas)
    {
        if (filmeId is <= 0)
        {
            ModelState.AddModelError(nameof(filmeId), "O filmeId deve ser maior que zero.");
            return ValidationProblem(ModelState);
        }

        var agora = DateTime.UtcNow;

        var consulta = _db.Sessoes.AsNoTracking().Include(s => s.Filme).AsQueryable();

        if (filmeId.HasValue)
            consulta = consulta.Where(s => s.FilmeId == filmeId.Value);

        if (apenasFuturas == true)
            consulta = consulta.Where(s => s.DataHora > agora);

        var sessoes = await consulta.OrderBy(s => s.DataHora).ToListAsync();
        var ids = sessoes.Select(s => s.Id).ToList();

        var ocupados = await _db.Reservas
            .AsNoTracking()
            .Where(r => ids.Contains(r.SessaoId) && r.Status == StatusReserva.Confirmada)
            .GroupBy(r => r.SessaoId)
            .Select(g => new { SessaoId = g.Key, Total = g.Sum(r => r.QuantidadeLugares) })
            .ToDictionaryAsync(x => x.SessaoId, x => x.Total);

        var lista = sessoes
            .Select(s => ParaResposta(s, ocupados.GetValueOrDefault(s.Id), agora))
            .ToList();

        if (apenasComLugares == true)
            lista = lista.Where(d => d.LugaresLivres > 0).ToList();

        return lista;
    }

    /// <summary>Consulta a disponibilidade de uma sessão.</summary>
    /// <param name="sessaoId">Id da sessão (consulte em GET /api/Sessoes).</param>
    /// <response code="200">Capacidade, lugares reservados e lugares livres da sessão.</response>
    /// <response code="404">Não existe sessão com esse Id.</response>
    [HttpGet("{sessaoId:int}")]
    [ProducesResponseType(typeof(DisponibilidadeResposta), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroResposta), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DisponibilidadeResposta>> BuscarPorSessao(int sessaoId)
    {
        var sessao = await _db.Sessoes
            .AsNoTracking()
            .Include(s => s.Filme)
            .FirstOrDefaultAsync(s => s.Id == sessaoId);

        if (sessao is null)
            return NotFound(new ErroResposta("Sessão não encontrada."));

        var ocupados = await _db.Reservas
            .Where(r => r.SessaoId == sessaoId && r.Status == StatusReserva.Confirmada)
            .SumAsync(r => r.QuantidadeLugares);

        return ParaResposta(sessao, ocupados, DateTime.UtcNow);
    }

    private static DisponibilidadeResposta ParaResposta(Sessao s, int reservados, DateTime agora)
    {
        var livres = Math.Max(0, s.Capacidade - reservados);

        return new DisponibilidadeResposta(
            s.Id,
            s.FilmeId,
            s.Filme.Titulo,
            s.Sala,
            s.DataHora,
            s.Preco,
            s.Capacidade,
            reservados,
            livres,
            livres == 0,
            s.DataHora <= agora
        );
    }
}