using System.Globalization;
using System.Text;

namespace CinemaWeb.Services;

/// <summary>Conversão entre UTC (como a API guarda) e o horário de Brasília (como o cliente vê).</summary>
public static class Fuso
{
    private static readonly TimeZoneInfo Brasilia = Obter();

    private static TimeZoneInfo Obter()
    {
        foreach (var id in new[] { "America/Sao_Paulo", "E. South America Standard Time" })
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
            catch (TimeZoneNotFoundException) { }
            catch (InvalidTimeZoneException) { }
        }
        return TimeZoneInfo.Utc;
    }

    public static DateTime ParaLocal(DateTime utc) =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), Brasilia);

    public static DateTime ParaUtc(DateTime local) =>
        TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(local, DateTimeKind.Unspecified), Brasilia);

    private static readonly CultureInfo PtBr = new("pt-BR");

    /// <summary>Ex.: "sex, 20/11 às 19:30".</summary>
    public static string Formatar(DateTime utc)
    {
        var l = ParaLocal(utc);
        return $"{l.ToString("ddd, dd/MM", PtBr)} às {l.ToString("HH:mm", PtBr)}";
    }

    /// <summary>Ex.: "20/11/2026 19:30".</summary>
    public static string FormatarCompleto(DateTime utc) =>
        ParaLocal(utc).ToString("dd/MM/yyyy HH:mm", PtBr);
}

public static class Texto
{
    /// <summary>Minúsculas e sem acentos, para comparar buscas ("acao" encontra "Ação").</summary>
    public static string Normalizar(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return "";
        var decomposto = texto.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decomposto.Length);
        foreach (var c in decomposto)
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    public static bool Contem(string? origem, string termoNormalizado) =>
        Normalizar(origem).Contains(termoNormalizado, StringComparison.Ordinal);
}

public static class Visual
{
    /// <summary>Escolhe sempre o mesmo tom de pôster para o mesmo gênero.</summary>
    public static string Tom(string? genero)
    {
        var g = Texto.Normalizar(genero);
        if (g == "terror") return "tom-terror";

        int h = 0;
        foreach (var c in g) h = (h * 31 + c) % 997;
        return $"tom-{h % 6}";
    }
}
