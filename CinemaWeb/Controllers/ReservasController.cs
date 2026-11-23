using CinemaWeb.Models;
using CinemaWeb.Models.Api;
using CinemaWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace CinemaWeb.Controllers;

public class ReservasController : Controller
{
    private readonly CinemaApiClient _api;

    public ReservasController(CinemaApiClient api) => _api = api;

    /// <summary>Minhas reservas: o cliente digita o e-mail e vê (e gerencia) as reservas dele.</summary>
    public async Task<IActionResult> Index(string? email)
    {
        var vm = new ReservasViewModel { Email = email?.Trim() };

        if (!string.IsNullOrWhiteSpace(vm.Email))
        {
            vm.Buscou = true;
            var r = await _api.ListarReservasAsync(email: vm.Email);
            if (r.Sucesso) vm.Reservas = r.Dados ?? new();
            else vm.Erro = r.MensagemGeral;
        }

        return View(vm);
    }

    // ------------------------------------------------------------- Nova reserva

    [HttpGet]
    public async Task<IActionResult> Nova(int? filmeId, int? sessaoId)
    {
        var vm = new ReservaFormViewModel { FilmeId = filmeId, SessaoId = sessaoId ?? 0 };

        // Veio só com a sessão: descobre o filme dela.
        if (sessaoId.HasValue && !filmeId.HasValue)
        {
            var d = await _api.BuscarDisponibilidadeAsync(sessaoId.Value);
            if (d.Sucesso && d.Dados is not null) vm.FilmeId = d.Dados.FilmeId;
        }

        await Preparar(vm);
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Nova(ReservaFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await Preparar(vm);
            return View(vm);
        }

        var r = await _api.CriarReservaAsync(new ReservaDto
        {
            SessaoId = vm.SessaoId,
            NomeCliente = vm.NomeCliente.Trim(),
            Email = vm.Email.Trim(),
            QuantidadeLugares = vm.QuantidadeLugares
        });

        if (!r.Sucesso)
        {
            // 400 (validação), 404 (sessão sumiu) ou 409 (esgotada / já começou)
            ModelState.AdicionarErrosApi(r);
            await Preparar(vm);
            return View(vm);
        }

        var reserva = r.Dados!;
        TempData["Sucesso"] =
            $"Reserva #{reserva.Id} confirmada: {reserva.QuantidadeLugares} lugar(es) para \"{reserva.TituloFilme}\" " +
            $"em {Fuso.Formatar(reserva.DataHoraSessao)}, total de {reserva.ValorTotal:C}.";

        return RedirectToAction(nameof(Index), new { email = vm.Email.Trim() });
    }

    // ------------------------------------------------- Ações sobre uma reserva

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id, string? email)
    {
        var r = await _api.CancelarReservaAsync(id);
        if (r.Sucesso) TempData["Sucesso"] = $"Reserva #{id} cancelada. Os lugares foram liberados.";
        else TempData["Erro"] = r.MensagemGeral;
        return RedirectToAction(nameof(Index), new { email });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Reativar(int id, string? email)
    {
        var r = await _api.ReativarReservaAsync(id);
        if (r.Sucesso) TempData["Sucesso"] = $"Reserva #{id} reativada.";
        else TempData["Erro"] = r.MensagemGeral; // ex.: 409 se os lugares já foram ocupados
        return RedirectToAction(nameof(Index), new { email });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Excluir(int id, string? email)
    {
        var r = await _api.ExcluirReservaAsync(id);
        if (r.Sucesso) TempData["Sucesso"] = $"Reserva #{id} excluída.";
        else TempData["Erro"] = r.MensagemGeral;
        return RedirectToAction(nameof(Index), new { email });
    }

    // ---------------------------------------------------------------- Auxiliares

    /// <summary>Carrega as sessões futuras com lugares livres e o filme (se houver).</summary>
    private async Task Preparar(ReservaFormViewModel vm)
    {
        var sessoes = await _api.ListarDisponibilidadeAsync(
            filmeId: vm.FilmeId, apenasComLugares: true, apenasFuturas: true);
        vm.Sessoes = sessoes.Dados ?? new();

        if (!sessoes.Sucesso)
            ModelState.AddModelError(string.Empty, sessoes.MensagemGeral);

        if (vm.FilmeId.HasValue)
        {
            var f = await _api.BuscarFilmeAsync(vm.FilmeId.Value);
            if (f.Sucesso) vm.Filme = f.Dados;
        }
    }
}
