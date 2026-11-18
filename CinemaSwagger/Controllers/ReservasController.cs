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

    /// <summary>Lista as reservas cadastradas, com filtros opcionais.</summary>
    /// <param name="sessaoId">Quando informado, lista somente reservas desta sessão.</param>
    /// <param name="email">Quando informado, lista somente reservas deste cliente.</param>
    /// <param name="status">Quando informado, filtra pela situação: Confirmada ou Cancelada.</param>
    /// <response code="200">Lista de reservas (pode estar vazia).</response>
    /// <response code="400">SessaoId ou status inválido.</response>
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

    /// <summary>Busca uma reserva pelo Id.</summary>
    /// <param name="id">Identificador da reserva.</param>
    /// <response code="200">Reserva encontrada.</response>
    /// <response code="404">Reserva não existe.</response>
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

    /// <summary>Cadastra uma reserva de lugares em uma sessão já existente.</summary>
    /// <remarks>
    /// Regras de negócio:
    /// a sessão não pode já ter começado e a quantidade pedida
    /// não pode passar dos lugares ainda livres (capacidade menos as reservas confirmadas).
    /// A reserva nasce com status Confirmada.
    /// O valor total é calculado como preço da sessão x quantidade de lugares.
    /// </remarks>
    /// <param name="dto">Dados da reserva a ser cadastrada.</param>
    /// <response code="201">Reserva criada; Location aponta para a consulta da reserva.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="404">Sessão não existe.</response>
    /// <response code="409">Sessão já começou ou não há lugares suficientes.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ReservaResposta), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReservaResposta>> Cadastrar(ReservaDto dto)
    {
        var sessao = await BuscarSessao(dto.SessaoId);

        if (sessao is null)
            return NotFound(new
            {
                mensagem = "Sessão não encontrada."
            });

        var erro = await ValidarRegras(sessao, dto.QuantidadeLugares);

        if (erro is not null)
            return Conflict(new
            {
                mensagem = erro
            });

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

    /// <summary>Atualiza todos os dados de uma reserva.</summary>
    /// <remarks>
    /// Os lugares da própria reserva não contam como ocupados na checagem de disponibilidade,
    /// então é possível aumentar ou diminuir a quantidade, ou mudar para outra sessão.
    /// O valor total é recalculado.
    /// </remarks>
    /// <param name="id">Identificador da reserva.</param>
    /// <param name="dto">Dados atualizados da reserva.</param>
    /// <response code="204">Reserva atualizada.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="404">Reserva ou sessão inexistente.</response>
    /// <response code="409">Reserva cancelada, sessão já começou ou não há lugares suficientes.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Atualizar(int id, ReservaDto dto)
    {
        var reserva = await _db.Reservas.FindAsync(id);

        if (reserva is null)
            return NotFound();

        if (reserva.Status == StatusReserva.Cancelada)
            return Conflict(new
            {
                mensagem = "Reserva cancelada não pode ser editada. Use /reativar primeiro."
            });

        var sessao = await BuscarSessao(dto.SessaoId);

        if (sessao is null)
            return NotFound(new
            {
                mensagem = "Sessão não encontrada."
            });

        var erro = await ValidarRegras(sessao, dto.QuantidadeLugares, id);

        if (erro is not null)
            return Conflict(new
            {
                mensagem = erro
            });

        Aplicar(reserva, dto, sessao);

        await _db.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>Regra de negócio: cancela uma reserva, liberando os lugares, sem apagá-la.</summary>
    /// <remarks>
    /// A reserva continua no histórico com status Cancelada.
    /// Não é possível cancelar uma reserva já cancelada nem de uma sessão que já começou.
    /// </remarks>
    /// <param name="id">Identificador da reserva.</param>
    /// <response code="200">Reserva cancelada; retorna a reserva atualizada.</response>
    /// <response code="404">Reserva não existe.</response>
    /// <response code="409">Reserva já cancelada ou sessão já começou.</response>
    [HttpPatch("{id:int}/cancelar")]
    [ProducesResponseType(typeof(ReservaResposta), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReservaResposta>> Cancelar(int id)
    {
        var reserva = await BuscarReservaCompleta(id);

        if (reserva is null)
            return NotFound();

        if (reserva.Status == StatusReserva.Cancelada)
            return Conflict(new
            {
                mensagem = "A reserva já está cancelada."
            });

        if (reserva.Sessao.DataHora <= DateTime.UtcNow)
            return Conflict(new
            {
                mensagem = "Esta sessão já começou; não é possível cancelar."
            });

        reserva.Status = StatusReserva.Cancelada;

        await _db.SaveChangesAsync();

        return ParaResposta(reserva);
    }

    /// <summary>Regra de negócio: reativa uma reserva cancelada, se ainda houver lugares.</summary>
    /// <remarks>
    /// Só funciona para reservas com status Cancelada.
    /// Os lugares são conferidos de novo, pois podem ter sido ocupados por outras reservas.
    /// </remarks>
    /// <param name="id">Identificador da reserva.</param>
    /// <response code="200">Reserva reativada; retorna a reserva atualizada.</response>
    /// <response code="404">Reserva não existe.</response>
    /// <response code="409">Reserva não está cancelada, sessão já começou ou não há lugares suficientes.</response>
    [HttpPatch("{id:int}/reativar")]
    [ProducesResponseType(typeof(ReservaResposta), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReservaResposta>> Reativar(int id)
    {
        var reserva = await BuscarReservaCompleta(id);

        if (reserva is null)
            return NotFound();

        if (reserva.Status != StatusReserva.Cancelada)
            return Conflict(new
            {
                mensagem = "Só é possível reativar uma reserva cancelada."
            });

        var erro = await ValidarRegras(reserva.Sessao, reserva.QuantidadeLugares, id);

        if (erro is not null)
            return Conflict(new
            {
                mensagem = erro
            });

        reserva.Status = StatusReserva.Confirmada;

        await _db.SaveChangesAsync();

        return ParaResposta(reserva);
    }

    /// <summary>Exclui uma reserva definitivamente pelo Id.</summary>
    /// <param name="id">Identificador da reserva.</param>
    /// <response code="204">Reserva excluída.</response>
    /// <response code="404">Reserva não existe.</response>
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
