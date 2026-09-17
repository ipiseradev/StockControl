using System.Net.Http.Json;

namespace StockControl.Web.Services;

public record Deposito(Guid Id, string Nombre, bool EsMostrador);
public record CrearDepositoRequest(string Nombre, bool EsMostrador);

public class DepositoService
{
    private readonly HttpClient _http;

    public DepositoService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<Deposito>> ObtenerTodos()
    {
        return await _http.GetFromJsonAsync<List<Deposito>>("depositos") ?? new();
    }

    public async Task<bool> Crear(CrearDepositoRequest req)
    {
        var response = await _http.PostAsJsonAsync("depositos", req);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> Actualizar(Guid id, CrearDepositoRequest req)
    {
        var response = await _http.PutAsJsonAsync($"depositos/{id}", req);
        return response.IsSuccessStatusCode;
    }

    public async Task<(bool exito, string? mensaje)> Eliminar(Guid id)
    {
        var response = await _http.DeleteAsync($"depositos/{id}");
        if (response.IsSuccessStatusCode) return (true, null);

        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        return (false, body?.GetValueOrDefault("mensaje") ?? "No se pudo eliminar.");
    }
}
