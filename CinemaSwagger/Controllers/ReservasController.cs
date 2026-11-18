using CinemaSwagger.Data;
using CinemaSwagger.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaSwagger.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ReservasController : ControllerBase
{
    private readonly AppDbContext _db;

    public ReservasController(AppDbContext db) => _db = db;

    /// <summary>Lista as reservas, com filtros opcionais.</summary>
    /// <remarks>Sem filtros, devolve todas as reservas, da mais recente para a mais antiga. Os filtros podem ser combinados.</remarks>
    /// <param name="sessaoId">Filtra pelo Id da sessão (consulte os Ids em GET /api/Sessoes).</param>
    /// <param name="email">Filtra pelo e-mail do cliente (ex.: ana.souza@email.com).</param>
    /// <param name="status">Filtra pela situação da reserva: Confirmada ou Cancelada.</param>
    /// <response code="200">Lista de reservas (pode estar vazia).</response>
    /// <response code="400">sessaoId menor ou igual a zero, ou status diferente de Confirmada/Cancelada.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<ReservaResposta>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<ReservaResposta>>> Listar(
        [FromQuery] int? sessaoId,
        [FromQuery] string? email,
        [FromQuery] string? status)
    {
        if (sessaoId is <= 0)
        {
            ModelState.AddModelError(
                nameof(sessaoId),
                "O sessaoId deve ser maior que zero."
            );

            return ValidationProblem(ModelState);
        }

        string? statusNormalizado = null;

        if (!string.IsNullOrWhiteSpace(status))
        {
            statusNormalizado = StatusReserva.Todos.FirstOrDefault(s =>
                s.Equals(status.Trim(), StringComparison.OrdinalIgnoreCase));

            if (statusNormalizado is null)
            {
                ModelState.AddModelError(
                    nameof(status),
                    "Use Confirmada ou Cancelada."
                );

                return ValidationProblem(ModelState);
            }
        }

        IQueryable<Reserva> consulta = _db.Reservas
            .AsNoTracking()
            .Include(r => r.Sessao)
            .ThenInclude(s => s.Filme);

        if (sessaoId.HasValue)
            consulta = consulta.Where(r => r.SessaoId == sessaoId.Value);

        if (statusNormalizado is not null)
            consulta = consulta.Where(r => r.Status == statusNormalizado);

        if (!string.IsNullOrWhiteSpace(email))
        {
            var emailNormalizado = email.Trim().ToLowerInvariant();
            consulta = consulta.Where(r => r.Email == emailNormalizado);
        }

        var reservas = await consulta
            .OrderByDescending(r => r.DataReserva)
            .ToListAsync();

        return reservas.Select(ParaResposta).ToList();
    }

    /// <summary>Consulta uma reserva pelo Id.</summary>
    /// <param name="id">Id da reserva (ex.: 1).</param>
    /// <response code="200">Reserva encontrada, com título do filme, sala e horário da sessão.</response>
    /// <response code="404">Não existe reserva com esse Id.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ReservaResposta), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReservaResposta>> BuscarPorId(int id)
    {
        var reserva = await _db.Reservas
            .AsNoTracking()
            .Include(r => r.Sessao)
            .ThenInclude(s => s.Filme)
            .FirstOrDefaultAsync(r => r.Id == id);

        return reserva is null
            ? NotFound()
            : ParaResposta(reserva);
    }

    /// <summary>Reserva lugares em uma sessão.</summary>
    /// <remarks>
    /// Antes de reservar, descubra o Id da sessão em GET /api/Sessoes.
    /// A reserva é criada com status Confirmada, e o valorTotal é calculado pela API
    /// (preço da sessão x quantidade de lugares).
    /// Só é aceita se a sessão ainda não começou e se houver lugares livres
    /// (capacidade da sessão menos as reservas confirmadas).
    /// </remarks>
    /// <param name="dto">Sessão, dados do cliente e quantidade de lugares.</param>
    /// <response code="201">Reserva criada. O cabeçalho Location aponta para a consulta dela.</response>
    /// <response code="400">Dados inválidos (ex.: nome vazio, e-mail inválido, quantidade fora de 1 a 10).</response>
    /// <response code="404">Não existe sessão com o sessaoId informado.</response>
    /// <response code="409">A sessão já começou ou não há lugares livres suficientes. A mensagem diz quantos restam.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ReservaResposta), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroResposta), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErroResposta), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReservaResposta>> Cadastrar(ReservaDto dto)
    {
        var sessao = await BuscarSessao(dto.SessaoId);

        if (sessao is null)
            return NotFound(new ErroResposta("Sessão não encontrada."));

        var erro = await ValidarRegras(sessao, dto.QuantidadeLugares);

        if (erro is not null)
            return Conflict(new ErroResposta(erro));

        var reserva = new Reserva
        {
            DataReserva = DateTime.UtcNow
        };

        Aplicar(reserva, dto, sessao);

        _db.Reservas.Add(reserva);

        await _db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(BuscarPorId),
            new { id = reserva.Id },
            ParaResposta(reserva)
        );
    }

    /// <summary>Altera os dados de uma reserva.</summary>
    /// <remarks>
    /// Envie todos os campos. Serve para trocar o nome, o e-mail, a quantidade de lugares ou a sessão;
    /// o valorTotal é recalculado. Os lugares da própria reserva não contam como ocupados na conferência.
    /// Reservas canceladas não podem ser alteradas: reative antes, em PATCH /api/Reservas/{id}/reativar.
    /// </remarks>
    /// <param name="id">Id da reserva que será alterada.</param>
    /// <param name="dto">Novos dados da reserva.</param>
    /// <response code="204">Reserva alterada (a resposta não tem corpo; consulte a reserva para ver o resultado).</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="404">Não existe reserva com esse Id, ou não existe sessão com o sessaoId informado.</response>
    /// <response code="409">A reserva está cancelada, a sessão já começou ou não há lugares livres suficientes.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErroResposta), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Atualizar(int id, ReservaDto dto)
    {
        var reserva = await _db.Reservas.FindAsync(id);

        if (reserva is null)
            return NotFound();

        if (reserva.Status == StatusReserva.Cancelada)
            return Conflict(new ErroResposta("Reserva cancelada não pode ser editada. Use /reativar primeiro."));

        var sessao = await BuscarSessao(dto.SessaoId);

        if (sessao is null)
            return NotFound(new ErroResposta("Sessão não encontrada."));

        var erro = await ValidarRegras(sessao, dto.QuantidadeLugares, id);

        if (erro is not null)
            return Conflict(new ErroResposta(erro));

        Aplicar(reserva, dto, sessao);

        await _db.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>Cancela uma reserva e libera os lugares.</summary>
    /// <remarks>
    /// A reserva não é apagada: continua na listagem com status Cancelada.
    /// Para apagá-la de vez, use DELETE. Não dá para cancelar uma reserva que já está cancelada
    /// nem de uma sessão que já começou.
    /// </remarks>
    /// <param name="id">Id da reserva que será cancelada.</param>
    /// <response code="200">Reserva cancelada. Retorna a reserva com status Cancelada.</response>
    /// <response code="404">Não existe reserva com esse Id.</response>
    /// <response code="409">A reserva já estava cancelada ou a sessão já começou.</response>
    [HttpPatch("{id:int}/cancelar")]
    [ProducesResponseType(typeof(ReservaResposta), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErroResposta), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReservaResposta>> Cancelar(int id)
    {
        var reserva = await BuscarReservaCompleta(id);

        if (reserva is null)
            return NotFound();

        if (reserva.Status == StatusReserva.Cancelada)
            return Conflict(new ErroResposta("A reserva já está cancelada."));

        if (reserva.Sessao.DataHora <= DateTime.UtcNow)
            return Conflict(new ErroResposta("Esta sessão já começou; não é possível cancelar."));

        reserva.Status = StatusReserva.Cancelada;

        await _db.SaveChangesAsync();

        return ParaResposta(reserva);
    }

    /// <summary>Reativa uma reserva que foi cancelada.</summary>
    /// <remarks>
    /// Só funciona para reservas com status Cancelada. Os lugares são conferidos de novo,
    /// porque podem ter sido ocupados por outras reservas enquanto ela esteve cancelada.
    /// </remarks>
    /// <param name="id">Id da reserva que será reativada.</param>
    /// <response code="200">Reserva reativada. Retorna a reserva com status Confirmada.</response>
    /// <response code="404">Não existe reserva com esse Id.</response>
    /// <response code="409">A reserva não está cancelada, a sessão já começou ou não há lugares livres suficientes.</response>
    [HttpPatch("{id:int}/reativar")]
    [ProducesResponseType(typeof(ReservaResposta), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErroResposta), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReservaResposta>> Reativar(int id)
    {
        var reserva = await BuscarReservaCompleta(id);

        if (reserva is null)
            return NotFound();

        if (reserva.Status != StatusReserva.Cancelada)
            return Conflict(new ErroResposta("Só é possível reativar uma reserva cancelada."));

        var erro = await ValidarRegras(reserva.Sessao, reserva.QuantidadeLugares, id);

        if (erro is not null)
            return Conflict(new ErroResposta(erro));

        reserva.Status = StatusReserva.Confirmada;

        await _db.SaveChangesAsync();

        return ParaResposta(reserva);
    }

    /// <summary>Apaga uma reserva definitivamente.</summary>
    /// <remarks>Diferente de cancelar, a reserva deixa de existir e não aparece mais na listagem.</remarks>
    /// <param name="id">Id da reserva que será apagada.</param>
    /// <response code="204">Reserva apagada (a resposta não tem corpo).</response>
    /// <response code="404">Não existe reserva com esse Id.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Excluir(int id)
    {
        var reserva = await _db.Reservas.FindAsync(id);

        if (reserva is null)
            return NotFound();

        _db.Reservas.Remove(reserva);

        await _db.SaveChangesAsync();

        return NoContent();
    }

    private Task<Reserva?> BuscarReservaCompleta(int id) =>
        _db.Reservas
            .Include(r => r.Sessao)
            .ThenInclude(s => s.Filme)
            .FirstOrDefaultAsync(r => r.Id == id);

    private Task<Sessao?> BuscarSessao(int sessaoId) =>
        _db.Sessoes
            .Include(s => s.Filme)
            .FirstOrDefaultAsync(s => s.Id == sessaoId);

    // Devolve a mensagem de erro da regra de negócio violada,
    // ou null quando a reserva pode ser feita.
    private async Task<string?> ValidarRegras(
        Sessao sessao,
        int quantidade,
        int? ignorarReservaId = null)
    {
        if (sessao.DataHora <= DateTime.UtcNow)
            return "Esta sessão já começou; não é possível reservar.";

        var ocupados = await _db.Reservas
            .Where(r =>
                r.SessaoId == sessao.Id &&
                r.Status == StatusReserva.Confirmada &&
                (!ignorarReservaId.HasValue || r.Id != ignorarReservaId.Value))
            .SumAsync(r => r.QuantidadeLugares);

        var livres = sessao.Capacidade - ocupados;

        if (quantidade > livres)
            return livres <= 0
                ? "A sessão está esgotada."
                : $"Só restam {livres} lugar(es) livre(s) nesta sessão.";

        return null;
    }

    private static void Aplicar(Reserva r, ReservaDto d, Sessao sessao)
    {
        r.SessaoId = sessao.Id;
        r.Sessao = sessao;
        r.NomeCliente = d.NomeCliente.Trim();
        r.Email = d.Email.Trim().ToLowerInvariant();
        r.QuantidadeLugares = d.QuantidadeLugares;
        r.ValorTotal = sessao.Preco * d.QuantidadeLugares;
    }

    private static ReservaResposta ParaResposta(Reserva r) => new(
        r.Id,
        r.SessaoId,
        r.Sessao.Filme.Titulo,
        r.Sessao.Sala,
        r.Sessao.DataHora,
        r.NomeCliente,
        r.Email,
        r.QuantidadeLugares,
        r.ValorTotal,
        r.Status,
        r.DataReserva
    );
}
