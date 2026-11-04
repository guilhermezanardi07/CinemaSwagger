using CinemaSwagger.Data;
using CinemaSwagger.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaSwagger.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FilmesController : ControllerBase
{
    private readonly AppDbContext _db;
    public FilmesController(AppDbContext db) => _db = db;

    /// <summary>Lista os filmes, com filtros opcionais.</summary>
    /// <param name="genero">Filtra pelo gênero (ex.: Animação).</param>
    /// <param name="emCartaz">true = em cartaz, false = fora de cartaz.</param>
    /// <response code="200">Lista de filmes (pode ser vazia).</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<Filme>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<Filme>>> Listar([FromQuery] string? genero, [FromQuery] bool? emCartaz)
    {
        var q = _db.Filmes.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(genero)) q = q.Where(f => f.Genero == genero);
        if (emCartaz.HasValue) q = q.Where(f => f.EmCartaz == emCartaz.Value);
        return await q.OrderBy(f => f.Titulo).ToListAsync();
    }

    /// <summary>Busca um filme pelo Id.</summary>
    /// <param name="id">Identificador do filme.</param>
    /// <response code="200">Filme encontrado.</response>
    /// <response code="404">Filme não existe.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Filme), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Filme>> BuscarPorId(int id)
    {
        var filme = await _db.Filmes.FindAsync(id);
        return filme is null ? NotFound() : filme;
    }

    /// <summary>Cadastra um novo filme.</summary>
    /// <response code="201">Filme criado; o cabeçalho Location aponta para ele.</response>
    /// <response code="400">Dados inválidos.</response>
    [HttpPost]
    [ProducesResponseType(typeof(Filme), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Filme>> Cadastrar(FilmeDto dto)
    {
        var filme = new Filme();
        Aplicar(filme, dto);
        _db.Filmes.Add(filme);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(BuscarPorId), new { id = filme.Id }, filme);
    }

    /// <summary>Atualiza todos os dados de um filme.</summary>
    /// <param name="id">Identificador do filme.</param>
    /// <response code="204">Atualizado com sucesso.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="404">Filme não existe.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(int id, FilmeDto dto)
    {
        var filme = await _db.Filmes.FindAsync(id);
        if (filme is null) return NotFound();
        Aplicar(filme, dto);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Exclui um filme.</summary>
    /// <param name="id">Identificador do filme.</param>
    /// <response code="204">Excluído com sucesso.</response>
    /// <response code="404">Filme não existe.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Excluir(int id)
    {
        var filme = await _db.Filmes.FindAsync(id);
        if (filme is null) return NotFound();
        _db.Filmes.Remove(filme);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Regra de negócio: coloca ou tira um filme de cartaz.</summary>
    /// <param name="id">Identificador do filme.</param>
    /// <param name="valor">true = colocar em cartaz; false = retirar.</param>
    /// <response code="200">Filme com a situação atualizada.</response>
    /// <response code="404">Filme não existe.</response>
    [HttpPatch("{id:int}/em-cartaz")]
    [ProducesResponseType(typeof(Filme), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Filme>> AlterarCartaz(int id, [FromQuery] bool valor)
    {
        var filme = await _db.Filmes.FindAsync(id);
        if (filme is null) return NotFound();
        filme.EmCartaz = valor;
        await _db.SaveChangesAsync();
        return filme;
    }

    private static void Aplicar(Filme f, FilmeDto d)
    {
        f.Titulo = d.Titulo;
        f.Genero = d.Genero;
        f.DuracaoMinutos = d.DuracaoMinutos;
        f.Classificacao = d.Classificacao;
        f.Sinopse = d.Sinopse;
        f.DataLancamento = d.DataLancamento;
        f.EmCartaz = d.EmCartaz;
    }
}