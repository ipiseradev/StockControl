using System.Net.Http.Json;

namespace StockControl.Web.Services;

public enum TipoMovimiento
{
    Entrada,
    Salida,
    Ajuste,
    Merma
}

public record Movimiento(Guid Id, Guid ProductoId, string ProductoNombre, Guid DepositoId, string DepositoNombre, TipoMovimiento Tipo, int Cantidad, DateTime Fecha, string? Nota);
public record CrearMovimientoRequest(Guid ProductoId, Guid DepositoId, TipoMovimiento Tipo, int Cantidad, string? Nota);

public class MovimientoService
{
    private readonly HttpClient _http;

    public MovimientoService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<Movimiento>> ObtenerTodos()
    {
        return await _http.GetFromJsonAsync<List<Movimiento>>("movimientos", JsonOptions.Default) ?? new();
    }

    public async Task<(bool exito, string? mensaje)> Crear(CrearMovimientoRequest req)
    {
        var response = await _http.PostAsJsonAsync("movimientos", req, JsonOptions.Default);
        if (response.IsSuccessStatusCode) return (true, null);

        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        return (false, body?.GetValueOrDefault("mensaje") ?? "No se pudo registrar el movimiento.");
    }
}
