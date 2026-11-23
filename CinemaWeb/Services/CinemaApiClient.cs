using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CinemaWeb.Models.Api;

namespace CinemaWeb.Services;

/// <summary>Faz todas as chamadas HTTP para a API CinemaSwagger.</summary>
public class CinemaApiClient
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _http;

    public CinemaApiClient(HttpClient http) => _http = http;

    // ---------------------------------------------------------------- Filmes

    public Task<ApiResultado<List<FilmeResposta>>> ListarFilmesAsync(bool? emCartaz = null, string? genero = null) =>
        Executar<List<FilmeResposta>>(() => _http.GetAsync("api/Filmes" + Q(("genero", genero), ("emCartaz", emCartaz))));

    public Task<ApiResultado<FilmeResposta>> BuscarFilmeAsync(int id) =>
        Executar<FilmeResposta>(() => _http.GetAsync($"api/Filmes/{id}"));

    public Task<ApiResultado<FilmeResposta>> CriarFilmeAsync(FilmeDto dto) =>
        Executar<FilmeResposta>(() => _http.PostAsJsonAsync("api/Filmes", dto, Json));

    public Task<ApiResultado<Vazio>> AtualizarFilmeAsync(int id, FilmeDto dto) =>
        Executar<Vazio>(() => _http.PutAsJsonAsync($"api/Filmes/{id}", dto, Json));

    public Task<ApiResultado<Vazio>> ExcluirFilmeAsync(int id) =>
        Executar<Vazio>(() => _http.DeleteAsync($"api/Filmes/{id}"));

    public Task<ApiResultado<FilmeResposta>> AlterarCartazAsync(int id, bool valor) =>
        Executar<FilmeResposta>(() => Patch($"api/Filmes/{id}/em-cartaz" + Q(("valor", valor))));

    // --------------------------------------------------------------- Sessões

    public Task<ApiResultado<List<SessaoResposta>>> ListarSessoesAsync(int? filmeId = null) =>
        Executar<List<SessaoResposta>>(() => _http.GetAsync("api/Sessoes" + Q(("filmeId", filmeId))));

    public Task<ApiResultado<SessaoResposta>> BuscarSessaoAsync(int id) =>
        Executar<SessaoResposta>(() => _http.GetAsync($"api/Sessoes/{id}"));

    public Task<ApiResultado<SessaoResposta>> CriarSessaoAsync(SessaoDto dto) =>
        Executar<SessaoResposta>(() => _http.PostAsJsonAsync("api/Sessoes", dto, Json));

    public Task<ApiResultado<Vazio>> AtualizarSessaoAsync(int id, SessaoDto dto) =>
        Executar<Vazio>(() => _http.PutAsJsonAsync($"api/Sessoes/{id}", dto, Json));

    public Task<ApiResultado<Vazio>> ExcluirSessaoAsync(int id) =>
        Executar<Vazio>(() => _http.DeleteAsync($"api/Sessoes/{id}"));

    // -------------------------------------------------------- Disponibilidade

    public Task<ApiResultado<List<DisponibilidadeResposta>>> ListarDisponibilidadeAsync(
        int? filmeId = null, bool? apenasComLugares = null, bool? apenasFuturas = null) =>
        Executar<List<DisponibilidadeResposta>>(() => _http.GetAsync("api/Disponibilidade" +
            Q(("filmeId", filmeId), ("apenasComLugares", apenasComLugares), ("apenasFuturas", apenasFuturas))));

    public Task<ApiResultado<DisponibilidadeResposta>> BuscarDisponibilidadeAsync(int sessaoId) =>
        Executar<DisponibilidadeResposta>(() => _http.GetAsync($"api/Disponibilidade/{sessaoId}"));

    // --------------------------------------------------------------- Reservas

    public Task<ApiResultado<List<ReservaResposta>>> ListarReservasAsync(
        string? email = null, int? sessaoId = null, string? status = null) =>
        Executar<List<ReservaResposta>>(() => _http.GetAsync("api/Reservas" +
            Q(("sessaoId", sessaoId), ("email", email), ("status", status))));

    public Task<ApiResultado<ReservaResposta>> CriarReservaAsync(ReservaDto dto) =>
        Executar<ReservaResposta>(() => _http.PostAsJsonAsync("api/Reservas", dto, Json));

    public Task<ApiResultado<ReservaResposta>> CancelarReservaAsync(int id) =>
        Executar<ReservaResposta>(() => Patch($"api/Reservas/{id}/cancelar"));

    public Task<ApiResultado<ReservaResposta>> ReativarReservaAsync(int id) =>
        Executar<ReservaResposta>(() => Patch($"api/Reservas/{id}/reativar"));

    public Task<ApiResultado<Vazio>> ExcluirReservaAsync(int id) =>
        Executar<Vazio>(() => _http.DeleteAsync($"api/Reservas/{id}"));

    // ------------------------------------------------------------- Internos

    private Task<HttpResponseMessage> Patch(string url) =>
        _http.SendAsync(new HttpRequestMessage(HttpMethod.Patch, url));

    /// <summary>Monta a query string ignorando parâmetros vazios.</summary>
    private static string Q(params (string Nome, object? Valor)[] parametros)
    {
        var partes = new List<string>();
        foreach (var (nome, valor) in parametros)
        {
            if (valor is null) continue;
            string texto = valor switch
            {
                bool b => b ? "true" : "false",
                string s => s,
                _ => Convert.ToString(valor, CultureInfo.InvariantCulture) ?? ""
            };
            if (string.IsNullOrWhiteSpace(texto)) continue;
            partes.Add($"{nome}={Uri.EscapeDataString(texto)}");
        }
        return partes.Count == 0 ? "" : "?" + string.Join("&", partes);
    }

    private static async Task<ApiResultado<T>> Executar<T>(Func<Task<HttpResponseMessage>> envio)
    {
        try
        {
            using var resposta = await envio();

            if (resposta.IsSuccessStatusCode)
            {
                T? dados = default;
                if (resposta.StatusCode != HttpStatusCode.NoContent && typeof(T) != typeof(Vazio))
                    dados = await resposta.Content.ReadFromJsonAsync<T>(Json);

                return new ApiResultado<T> { Sucesso = true, Status = (int)resposta.StatusCode, Dados = dados };
            }

            return await LerFalha<T>(resposta);
        }
        catch (HttpRequestException)
        {
            return FalhaDeConexao<T>();
        }
        catch (TaskCanceledException)
        {
            return FalhaDeConexao<T>();
        }
        catch (JsonException)
        {
            return new ApiResultado<T> { Sucesso = false, Status = 0, Mensagem = "A API respondeu em um formato inesperado." };
        }
    }

    private static ApiResultado<T> FalhaDeConexao<T>() => new()
    {
        Sucesso = false,
        Status = 0,
        Mensagem = "Não foi possível falar com a API. Confira se o projeto CinemaSwagger está em execução."
    };

    /// <summary>Lê o corpo de erro: "errors" (400, ValidationProblemDetails) ou "mensagem" (404/409, ErroResposta).</summary>
    private static async Task<ApiResultado<T>> LerFalha<T>(HttpResponseMessage resposta)
    {
        string? mensagem = null;
        var erros = new Dictionary<string, string[]>();

        try
        {
            var corpo = await resposta.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(corpo))
            {
                using var doc = JsonDocument.Parse(corpo);
                if (doc.RootElement.ValueKind == JsonValueKind.Object)
                {
                    foreach (var prop in doc.RootElement.EnumerateObject())
                    {
                        if (prop.Name.Equals("mensagem", StringComparison.OrdinalIgnoreCase)
                            && prop.Value.ValueKind == JsonValueKind.String)
                        {
                            mensagem = prop.Value.GetString();
                        }
                        else if (prop.Name.Equals("errors", StringComparison.OrdinalIgnoreCase)
                                 && prop.Value.ValueKind == JsonValueKind.Object)
                        {
                            foreach (var campo in prop.Value.EnumerateObject())
                            {
                                if (campo.Value.ValueKind != JsonValueKind.Array) continue;
                                erros[campo.Name] = campo.Value.EnumerateArray()
                                    .Where(v => v.ValueKind == JsonValueKind.String)
                                    .Select(v => v.GetString() ?? "")
                                    .ToArray();
                            }
                        }
                    }
                }
            }
        }
        catch (JsonException)
        {
            // corpo não era JSON; usa a mensagem padrão abaixo
        }

        if (mensagem is null && erros.Count == 0)
        {
            mensagem = resposta.StatusCode switch
            {
                HttpStatusCode.NotFound => "Registro não encontrado.",
                HttpStatusCode.BadRequest => "Dados inválidos.",
                HttpStatusCode.Conflict => "A operação conflita com uma regra do cinema.",
                _ => $"A API respondeu com erro {(int)resposta.StatusCode}."
            };
        }

        return new ApiResultado<T>
        {
            Sucesso = false,
            Status = (int)resposta.StatusCode,
            Mensagem = mensagem,
            Erros = erros
        };
    }
}
