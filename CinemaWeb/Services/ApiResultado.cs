using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CinemaWeb.Services;

/// <summary>Resultado de uma chamada à API: sucesso ou os motivos do erro.</summary>
public class ApiResultado
{
    public bool Sucesso { get; init; }

    /// <summary>Código HTTP devolvido pela API (0 = não conseguiu falar com a API).</summary>
    public int Status { get; init; }

    /// <summary>Mensagem do campo "mensagem" (erros 404/409) ou texto padrão.</summary>
    public string? Mensagem { get; init; }

    /// <summary>Erros de validação (400) no formato campo → mensagens.</summary>
    public Dictionary<string, string[]> Erros { get; init; } = new();

    public string MensagemGeral
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(Mensagem)) return Mensagem!;
            if (Erros.Count > 0) return string.Join(" ", Erros.SelectMany(e => e.Value));
            return "Ocorreu um erro inesperado.";
        }
    }
}

public class ApiResultado<T> : ApiResultado
{
    public T? Dados { get; init; }
}

/// <summary>Usado nas chamadas que não devolvem conteúdo (204).</summary>
public sealed class Vazio { }

public static class ApiResultadoExtensions
{
    /// <summary>Coloca os erros devolvidos pela API no ModelState, para aparecerem no formulário.</summary>
    public static void AdicionarErrosApi(this ModelStateDictionary modelState, ApiResultado resultado)
    {
        if (resultado.Erros.Count > 0)
        {
            foreach (var mensagem in resultado.Erros.SelectMany(e => e.Value))
                modelState.AddModelError(string.Empty, mensagem);
        }
        else
        {
            modelState.AddModelError(string.Empty, resultado.MensagemGeral);
        }
    }
}
