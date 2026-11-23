using System.ComponentModel.DataAnnotations;
using CinemaWeb.Models.Api;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace CinemaWeb.Models;

public record GeneroGrupo(string Nome, List<FilmeResposta> Filmes);

public class HomeViewModel
{
    public List<FilmeResposta> Destaques { get; set; } = new();
    public List<GeneroGrupo> Generos { get; set; } = new();
    public string? Erro { get; set; }
}

public class CatalogoViewModel
{
    public string? Genero { get; set; }
    public List<string> TodosGeneros { get; set; } = new();
    public List<FilmeResposta> Filmes { get; set; } = new();
    public string? Erro { get; set; }
}

public class BuscaViewModel
{
    public string Termo { get; set; } = "";
    public List<FilmeResposta> Filmes { get; set; } = new();
    public string? Erro { get; set; }
}

public class FilmeDetalhesViewModel
{
    public FilmeResposta Filme { get; set; } = new();
    public List<DisponibilidadeResposta> Sessoes { get; set; } = new();
    public string? ErroSessoes { get; set; }
}

public class SessoesViewModel
{
    public int? FilmeId { get; set; }
    public List<FilmeResposta> Filmes { get; set; } = new();
    public List<DisponibilidadeResposta> Sessoes { get; set; } = new();
    public string? Erro { get; set; }
}

public class SessaoFormViewModel
{
    [Display(Name = "Filme")]
    [Range(1, int.MaxValue, ErrorMessage = "Escolha um filme.")]
    public int FilmeId { get; set; }

    [Display(Name = "Sala")]
    [Required(ErrorMessage = "Informe a sala.")]
    [StringLength(30, MinimumLength = 1, ErrorMessage = "A sala deve ter até 30 caracteres.")]
    public string Sala { get; set; } = "";

    /// <summary>Data e hora no horário de Brasília (convertida para UTC antes de ir para a API).</summary>
    [Display(Name = "Data e horário (Brasília)")]
    public DateTime DataHoraLocal { get; set; } = DateTime.Today.AddDays(1).AddHours(19);

    [Display(Name = "Preço do ingresso (R$)")]
    [Range(0, 10000, ErrorMessage = "O preço deve estar entre 0 e 10000 reais.")]
    public decimal Preco { get; set; } = 25;

    [Display(Name = "Capacidade (lugares)")]
    [Range(1, 1000, ErrorMessage = "A capacidade deve ser de 1 a 1000 lugares.")]
    public int Capacidade { get; set; } = 80;

    [ValidateNever]
    public List<FilmeResposta> Filmes { get; set; } = new();
}

public class SessoesAdminViewModel
{
    public List<SessaoResposta> Sessoes { get; set; } = new();
    public string? Erro { get; set; }
}

public class ReservaFormViewModel
{
    /// <summary>Filme escolhido na tela anterior (só serve para filtrar as sessões).</summary>
    public int? FilmeId { get; set; }

    [Display(Name = "Sessão")]
    [Range(1, int.MaxValue, ErrorMessage = "Escolha uma sessão.")]
    public int SessaoId { get; set; }

    [Display(Name = "Nome completo")]
    [Required(ErrorMessage = "Informe o seu nome.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter de 2 a 100 caracteres.")]
    public string NomeCliente { get; set; } = "";

    [Display(Name = "E-mail")]
    [Required(ErrorMessage = "Informe o seu e-mail.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [StringLength(150)]
    public string Email { get; set; } = "";

    [Display(Name = "Quantidade de lugares")]
    [Range(1, 10, ErrorMessage = "Reserve de 1 a 10 lugares por vez.")]
    public int QuantidadeLugares { get; set; } = 1;

    [ValidateNever]
    public FilmeResposta? Filme { get; set; }

    [ValidateNever]
    public List<DisponibilidadeResposta> Sessoes { get; set; } = new();
}

public class ReservasViewModel
{
    public string? Email { get; set; }
    public bool Buscou { get; set; }
    public List<ReservaResposta> Reservas { get; set; } = new();
    public string? Erro { get; set; }
}
