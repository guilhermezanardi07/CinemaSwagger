
using CinemaSwagger.Data;
using CinemaSwagger.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaSwagger.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SessoesController : ControllerBase
{
    private readonly AppDbContext _db;

    public SessoesController(AppDbContext db) => _db = db;

    /// <summary>Lista as sessões cadastradas, com filtro opcional por filme.</summary>
    /// <param name="filmeId">Quando informado, lista somente sessões deste filme.</param>
    /// <response code="200">Lista de sessões (pode estar vazia).</response>
    /// <response code="400">FilmeId inválido.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<SessaoResposta>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<SessaoResposta>>> Listar(
        [FromQuery] int? filmeId)
    {
        if (filmeId is <= 0)
        {
            ModelState.AddModelError(
                nameof(filmeId),
                "O filmeId deve ser maior que zero."
            );

            return ValidationProblem(ModelState);
        }

        var consulta = _db.Sessoes
            .AsNoTracking()
            .Include(s => s.Filme)
            .AsQueryable();

        if (filmeId.HasValue)
            consulta = consulta.Where(s => s.FilmeId == filmeId.Value);

        var sessoes = await consulta
            .OrderBy(s => s.DataHora)
            .ToListAsync();

        return sessoes.Select(ParaResposta).ToList();
    }

    /// <summary>Busca uma sessão pelo Id.</summary>
    /// <param name="id">Identificador da sessão.</param>
    /// <response code="200">Sessão encontrada.</response>
    /// <response code="404">Sessão não existe.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(SessaoResposta), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SessaoResposta>> BuscarPorId(int id)
    {
        var sessao = await _db.Sessoes
            .AsNoTracking()
            .Include(s => s.Filme)
            .FirstOrDefaultAsync(s => s.Id == id);

        return sessao is null
            ? NotFound()
            : ParaResposta(sessao);
    }

    /// <summary>Cadastra uma sessão vinculada a um filme já existente.</summary>
    /// <remarks>
    /// Use UTC no campo dataHora, com Z.
    /// A mesma sala não pode ter horários sobrepostos.
    /// O fim da sessão é calculado usando a duração do filme.
    /// </remarks>
    /// <param name="dto">Dados da sessão a ser cadastrada.</param>
    /// <response code="201">Sessão criada; Location aponta para a consulta da sessão.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="404">Filme não existe.</response>
    /// <response code="409">Já existe uma exibição na sala no intervalo.</response>
    [HttpPost]
    [ProducesResponseType(typeof(SessaoResposta), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SessaoResposta>> Cadastrar(SessaoDto dto)
    {
        if (!EntradaValida(dto))
            return ValidationProblem(ModelState);

        var filme = await _db.Filmes.FindAsync(dto.FilmeId);

        if (filme is null)
            return NotFound(new
            {
                mensagem = "Filme não encontrado."
            });

        if (await ExisteConflito(dto, filme.DuracaoMinutos))
            return Conflict(new
            {
                mensagem = "A sala já possui outra sessão neste intervalo."
            });

        var sessao = new Sessao();

        Aplicar(sessao, dto);

        sessao.Filme = filme;

        _db.Sessoes.Add(sessao);

        await _db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(BuscarPorId),
            new { id = sessao.Id },
            ParaResposta(sessao)
        );
    }

    /// <summary>Atualiza todos os dados de uma sessão.</summary>
    /// <param name="id">Identificador da sessão.</param>
    /// <param name="dto">Dados atualizados da sessão.</param>
    /// <response code="204">Sessão atualizada.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="404">Sessão ou filme inexistente.</response>
    /// <response code="409">Horário em conflito com outra sessão na mesma sala.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Atualizar(int id, SessaoDto dto)
    {
        var sessao = await _db.Sessoes.FindAsync(id);

        if (sessao is null)
            return NotFound();

        if (!EntradaValida(dto))
            return ValidationProblem(ModelState);

        var filme = await _db.Filmes.FindAsync(dto.FilmeId);

        if (filme is null)
            return NotFound(new
            {
                mensagem = "Filme não encontrado."
            });

        if (await ExisteConflito(dto, filme.DuracaoMinutos, id))
            return Conflict(new
            {
                mensagem = "A sala já possui outra sessão neste intervalo."
            });

        Aplicar(sessao, dto);

        sessao.Filme = filme;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>Exclui uma sessão pelo Id.</summary>
    /// <param name="id">Identificador da sessão.</param>
    /// <response code="204">Sessão excluída.</response>
    /// <response code="404">Sessão não existe.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Excluir(int id)
    {
        var sessao = await _db.Sessoes.FindAsync(id);

        if (sessao is null)
            return NotFound();

        _db.Sessoes.Remove(sessao);

        await _db.SaveChangesAsync();

        return NoContent();
    }

    private bool EntradaValida(SessaoDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Sala))
        {
            ModelState.AddModelError(
                nameof(dto.Sala),
                "Informe uma sala não vazia."
            );
        }

        if (dto.DataHora == default ||
            dto.DataHora.Kind != DateTimeKind.Utc)
        {
            ModelState.AddModelError(
                nameof(dto.DataHora),
                "Informe dataHora em UTC com Z (ex.: 2026-12-15T22:30:00Z)."
            );
        }

        return ModelState.IsValid;
    }

    // Compara a duração dos filmes para impedir sessões
    // simultâneas na mesma sala.
    private async Task<bool> ExisteConflito(
        SessaoDto dto,
        int duracaoNovoFilme,
        int? ignorarId = null)
    {
        var sala = dto.Sala.Trim().ToUpperInvariant();

        var fimNovo = dto.DataHora.AddMinutes(duracaoNovoFilme);

        var existentes = await _db.Sessoes
            .AsNoTracking()
            .Include(s => s.Filme)
            .Where(s =>
                s.Sala.ToUpper() == sala &&
                (!ignorarId.HasValue || s.Id != ignorarId.Value))
            .ToListAsync();

        return existentes.Any(s =>
            s.DataHora < fimNovo &&
            dto.DataHora <
                s.DataHora.AddMinutes(s.Filme.DuracaoMinutos)
        );
    }

    private static void Aplicar(Sessao s, SessaoDto d)
    {
        s.FilmeId = d.FilmeId;
        s.Sala = d.Sala.Trim();
        s.DataHora = d.DataHora;
        s.Preco = d.Preco;
        s.Capacidade = d.Capacidade;
    }

    private static SessaoResposta ParaResposta(Sessao s) => new(
        s.Id,
        s.FilmeId,
        s.Filme.Titulo,
        s.Sala,
        s.DataHora,
        s.Preco,
        s.Capacidade
    );
}
