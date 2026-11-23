using System.Diagnostics;
using CinemaWeb.Models;
using CinemaWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace CinemaWeb.Controllers;

public class HomeController : Controller
{
    private readonly CinemaApiClient _api;

    public HomeController(CinemaApiClient api) => _api = api;

    /// <summary>Tela inicial: carrossel de destaques, manifesto e filmes por gênero.</summary>
    public async Task<IActionResult> Index()
    {
        var vm = new HomeViewModel();
        var r = await _api.ListarFilmesAsync(emCartaz: true);

        if (!r.Sucesso)
        {
            vm.Erro = r.MensagemGeral;
            return View(vm);
        }

        var filmes = r.Dados ?? new();

        vm.Destaques = filmes
            .OrderByDescending(f => f.DataLancamento)
            .Take(5)
            .ToList();

        vm.Generos = filmes
            .GroupBy(f => f.Genero)
            .OrderBy(g => g.Key)
            .Select(g => new GeneroGrupo(g.Key, g.OrderBy(f => f.Titulo).ToList()))
            .ToList();

        return View(vm);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
