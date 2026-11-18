using CinemaSwagger.Data;
using CinemaSwagger.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CinemaSwagger.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ReservasController : ControllerBase
{
    private readonly AppDbContext _db;
    public ReservasController(AppDbContext db) => _db = db;

    /// <summary>Reserva um assento ainda livre em uma sessão.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ReservaResposta), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReservaResposta>> Cadastrar(ReservaDto dto)
    {
        var sessao = await _db.Sessoes.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == dto.SessaoId);
        if (sessao is null) return NotFound(new { mensagem = "Sessão não encontrada." });

        var assento = dto.Assento.Trim();
        if (!AssentoDentroDaCapacidade(assento, sessao.Capacidade))
        {
            ModelState.AddModelError(nameof(dto.Assento), "O número do assento excede a capacidade da sessão.");
            return ValidationProblem(ModelState);
        }

        if (await _db.Reservas.AnyAsync(r => r.SessaoId == dto.SessaoId && r.Assento == assento))
            return Conflict(new { mensagem = "Este assento já está reservado para a sessão." });

        var reserva = new Reserva { SessaoId = dto.SessaoId, Assento = assento, CriadaEm = DateTime.UtcNow };
        _db.Reservas.Add(reserva);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation
        })
        {
            // O índice único protege também contra duas reservas simultâneas do mesmo assento.
            return Conflict(new { mensagem = "Este assento já está reservado para a sessão." });
        }

        var resposta = ParaResposta(reserva);
        return Created($"/api/Reservas/{reserva.Id}", resposta);
    }

    /// <summary>Lista as reservas de uma sessão.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ReservaResposta>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ReservaResposta>>> Listar([FromQuery] int sessaoId)
    {
        if (sessaoId <= 0)
        {
            ModelState.AddModelError(nameof(sessaoId), "Informe um sessaoId maior que zero.");
            return ValidationProblem(ModelState);
        }
        var reservas = await _db.Reservas.AsNoTracking().Where(r => r.SessaoId == sessaoId)
            .OrderBy(r => r.Assento).ToListAsync();
        return reservas.Select(ParaResposta).ToList();
    }

    /// <summary>Cancela uma reserva pelo identificador.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancelar(int id)
    {
        var reserva = await _db.Reservas.FindAsync(id);
        if (reserva is null) return NotFound();
        _db.Reservas.Remove(reserva);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static bool AssentoDentroDaCapacidade(string assento, int capacidade)
    {
        return int.TryParse(assento, out var numero) && numero >= 1 && numero <= capacidade;
    }

    private static ReservaResposta ParaResposta(Reserva r) => new(r.Id, r.SessaoId, r.Assento, r.CriadaEm);
}
