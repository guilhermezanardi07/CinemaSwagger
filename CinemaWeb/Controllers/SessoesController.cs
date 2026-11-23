using CinemaWeb.Models;
using CinemaWeb.Models.Api;
using CinemaWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace CinemaWeb.Controllers;

public class SessoesController : Controller
{
    private readonly CinemaApiClient _api;

    public SessoesController(CinemaApiClient api) => _api = api;

    /// <summary>Programação: próximas sessões com lugares livres, com filtro por filme.</summary>
    public async Task<IActionResult> Index(int? filmeId)
    {
        var vm = new SessoesViewModel { FilmeId = filmeId };

        var filmes = await _api.ListarFilmesAsync(emCartaz: true);
        if (filmes.Sucesso) vm.Filmes = (filmes.Dados ?? new()).OrderBy(f => f.Titulo).ToList();

        var r = await _api.ListarDisponibilidadeAsync(filmeId: filmeId, apenasFuturas: true);
        if (r.Sucesso) vm.Sessoes = r.Dados ?? new();
        else vm.Erro = r.MensagemGeral;

        return View(vm);
    }

    // ------------------------------------------------------ Administração de sessões

    public async Task<IActionResult> Gerenciar()
    {
        var vm = new SessoesAdminViewModel();
        var r = await _api.ListarSessoesAsync();
        if (r.Sucesso) vm.Sessoes = r.Dados ?? new();
        else vm.Erro = r.MensagemGeral;
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Criar(int? filmeId)
    {
        ViewData["Id"] = null;
        var vm = new SessaoFormViewModel { FilmeId = filmeId ?? 0 };
        await CarregarFilmes(vm);
        return View("Formulario", vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(SessaoFormViewModel vm)
    {
        ViewData["Id"] = null;
        if (!ModelState.IsValid)
        {
            await CarregarFilmes(vm);
            return View("Formulario", vm);
        }

        var r = await _api.CriarSessaoAsync(ParaDto(vm));
        if (!r.Sucesso)
        {
            ModelState.AdicionarErrosApi(r);
            await CarregarFilmes(vm);
            return View("Formulario", vm);
        }

        TempData["Sucesso"] = "Sessão cadastrada.";
        return RedirectToAction(nameof(Gerenciar));
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var r = await _api.BuscarSessaoAsync(id);
        if (!r.Sucesso || r.Dados is null)
        {
            if (r.Status == 404) return NotFound();
            TempData["Erro"] = r.MensagemGeral;
            return RedirectToAction(nameof(Gerenciar));
        }

        var s = r.Dados;
        var vm = new SessaoFormViewModel
        {
            FilmeId = s.FilmeId,
            Sala = s.Sala,
            DataHoraLocal = Fuso.ParaLocal(s.DataHora),
            Preco = s.Preco,
            Capacidade = s.Capacidade
        };

        ViewData["Id"] = id;
        await CarregarFilmes(vm);
        return View("Formulario", vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, SessaoFormViewModel vm)
    {
        ViewData["Id"] = id;
        if (!ModelState.IsValid)
        {
            await CarregarFilmes(vm);
            return View("Formulario", vm);
        }

        var r = await _api.AtualizarSessaoAsync(id, ParaDto(vm));
        if (!r.Sucesso)
        {
            if (r.Status == 404 && r.Mensagem is null) return NotFound();
            ModelState.AdicionarErrosApi(r);
            await CarregarFilmes(vm);
            return View("Formulario", vm);
        }

        TempData["Sucesso"] = "Sessão atualizada.";
        return RedirectToAction(nameof(Gerenciar));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Excluir(int id)
    {
        var r = await _api.ExcluirSessaoAsync(id);
        if (r.Sucesso) TempData["Sucesso"] = "Sessão excluída (as reservas dela também).";
        else TempData["Erro"] = r.MensagemGeral;
        return RedirectToAction(nameof(Gerenciar));
    }

    // ---------------------------------------------------------------- Auxiliares

    private async Task CarregarFilmes(SessaoFormViewModel vm)
    {
        var r = await _api.ListarFilmesAsync();
        vm.Filmes = (r.Dados ?? new()).OrderBy(f => f.Titulo).ToList();
    }

    /// <summary>O formulário trabalha em horário de Brasília; a API espera UTC.</summary>
    private static SessaoDto ParaDto(SessaoFormViewModel vm) => new()
    {
        FilmeId = vm.FilmeId,
        Sala = vm.Sala.Trim(),
        DataHora = Fuso.ParaUtc(vm.DataHoraLocal),
        Preco = vm.Preco,
        Capacidade = vm.Capacidade
    };
}
