using CinemaWeb.Models;
using CinemaWeb.Models.Api;
using CinemaWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace CinemaWeb.Controllers;

public class FilmesController : Controller
{
    private readonly CinemaApiClient _api;

    public FilmesController(CinemaApiClient api) => _api = api;

    // ------------------------------------------------------------ Telas públicas

    /// <summary>Catálogo: todos os filmes em cartaz, com filtro por gênero.</summary>
    public async Task<IActionResult> Index(string? genero)
    {
        var vm = new CatalogoViewModel { Genero = genero };
        var r = await _api.ListarFilmesAsync(emCartaz: true);

        if (!r.Sucesso)
        {
            vm.Erro = r.MensagemGeral;
            return View(vm);
        }

        var filmes = r.Dados ?? new();
        vm.TodosGeneros = filmes.Select(f => f.Genero).Distinct().OrderBy(g => g).ToList();

        vm.Filmes = string.IsNullOrWhiteSpace(genero)
            ? filmes.OrderBy(f => f.Titulo).ToList()
            : filmes.Where(f => Texto.Normalizar(f.Genero) == Texto.Normalizar(genero))
                    .OrderBy(f => f.Titulo).ToList();

        return View(vm);
    }

    /// <summary>Resultado da barra de pesquisa: busca pelo nome ou pela categoria.</summary>
    public async Task<IActionResult> Buscar(string? termo)
    {
        var vm = new BuscaViewModel { Termo = termo?.Trim() ?? "" };
        var r = await _api.ListarFilmesAsync(emCartaz: true);

        if (!r.Sucesso)
        {
            vm.Erro = r.MensagemGeral;
            return View(vm);
        }

        vm.Filmes = Filtrar(r.Dados ?? new(), vm.Termo);
        return View(vm);
    }

    /// <summary>Sugestões que aparecem enquanto a pessoa digita (JSON).</summary>
    public async Task<IActionResult> Sugestoes(string? termo)
    {
        if (string.IsNullOrWhiteSpace(termo)) return Json(Array.Empty<object>());

        var r = await _api.ListarFilmesAsync(emCartaz: true);
        if (!r.Sucesso) return Json(Array.Empty<object>());

        var lista = Filtrar(r.Dados ?? new(), termo)
            .Take(6)
            .Select(f => new { id = f.Id, titulo = f.Titulo, genero = f.Genero });

        return Json(lista);
    }

    /// <summary>Página do filme (botão "Saiba mais"), com as sessões e lugares livres.</summary>
    public async Task<IActionResult> Detalhes(int id)
    {
        var r = await _api.BuscarFilmeAsync(id);
        if (!r.Sucesso || r.Dados is null)
        {
            if (r.Status == 404) return NotFound();
            TempData["Erro"] = r.MensagemGeral;
            return RedirectToAction("Index", "Home");
        }

        var vm = new FilmeDetalhesViewModel { Filme = r.Dados };

        var sessoes = await _api.ListarDisponibilidadeAsync(filmeId: id, apenasFuturas: true);
        if (sessoes.Sucesso) vm.Sessoes = sessoes.Dados ?? new();
        else vm.ErroSessoes = sessoes.MensagemGeral;

        return View(vm);
    }

    // ------------------------------------------------------ Administração de filmes

    public async Task<IActionResult> Gerenciar()
    {
        var r = await _api.ListarFilmesAsync();
        if (!r.Sucesso) ViewData["Erro"] = r.MensagemGeral;
        return View(r.Dados ?? new List<FilmeResposta>());
    }

    [HttpGet]
    public IActionResult Criar()
    {
        ViewData["Id"] = null;
        return View("Formulario", new FilmeDto());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(FilmeDto dto)
    {
        ViewData["Id"] = null;
        if (!ModelState.IsValid) return View("Formulario", dto);

        var r = await _api.CriarFilmeAsync(dto);
        if (!r.Sucesso)
        {
            ModelState.AdicionarErrosApi(r);
            return View("Formulario", dto);
        }

        TempData["Sucesso"] = $"Filme \"{dto.Titulo}\" cadastrado.";
        return RedirectToAction(nameof(Gerenciar));
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var r = await _api.BuscarFilmeAsync(id);
        if (!r.Sucesso || r.Dados is null)
        {
            if (r.Status == 404) return NotFound();
            TempData["Erro"] = r.MensagemGeral;
            return RedirectToAction(nameof(Gerenciar));
        }

        var f = r.Dados;
        ViewData["Id"] = id;
        return View("Formulario", new FilmeDto
        {
            Titulo = f.Titulo,
            Genero = f.Genero,
            DuracaoMinutos = f.DuracaoMinutos,
            Classificacao = f.Classificacao,
            Sinopse = f.Sinopse,
            DataLancamento = f.DataLancamento,
            EmCartaz = f.EmCartaz
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, FilmeDto dto)
    {
        ViewData["Id"] = id;
        if (!ModelState.IsValid) return View("Formulario", dto);

        var r = await _api.AtualizarFilmeAsync(id, dto);
        if (!r.Sucesso)
        {
            if (r.Status == 404) return NotFound();
            ModelState.AdicionarErrosApi(r);
            return View("Formulario", dto);
        }

        TempData["Sucesso"] = $"Filme \"{dto.Titulo}\" atualizado.";
        return RedirectToAction(nameof(Gerenciar));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Excluir(int id)
    {
        var r = await _api.ExcluirFilmeAsync(id);
        if (r.Sucesso) TempData["Sucesso"] = "Filme excluído.";
        else TempData["Erro"] = r.MensagemGeral; // ex.: 409 quando o filme tem sessões
        return RedirectToAction(nameof(Gerenciar));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AlterarCartaz(int id, bool valor)
    {
        var r = await _api.AlterarCartazAsync(id, valor);
        if (r.Sucesso) TempData["Sucesso"] = valor ? "Filme colocado em cartaz." : "Filme retirado de cartaz.";
        else TempData["Erro"] = r.MensagemGeral;
        return RedirectToAction(nameof(Gerenciar));
    }

    // ---------------------------------------------------------------- Auxiliares

    private static List<FilmeResposta> Filtrar(List<FilmeResposta> filmes, string termo)
    {
        var t = Texto.Normalizar(termo);
        if (t.Length == 0) return new();

        return filmes
            .Where(f => Texto.Contem(f.Titulo, t) || Texto.Contem(f.Genero, t))
            .OrderBy(f => f.Titulo)
            .ToList();
    }
}
