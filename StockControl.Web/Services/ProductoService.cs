using System.Net.Http.Json;

namespace StockControl.Web.Services;

public record Producto(Guid Id, string Nombre, string? CodigoBarras, string? Sku, decimal PrecioVenta, int StockMinimo, int StockActual);
public record CrearProductoRequest(string Nombre, string? CodigoBarras, string? Sku, decimal PrecioVenta, int StockMinimo);

public class ProductoService
{
    private readonly HttpClient _http;

    public ProductoService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<Producto>> ObtenerTodos()
    {
        return await _http.GetFromJsonAsync<List<Producto>>("productos") ?? new();
    }

    public async Task<List<Producto>> ObtenerAlertas()
    {
        return await _http.GetFromJsonAsync<List<Producto>>("productos/alertas") ?? new();
    }

    public async Task<bool> Crear(CrearProductoRequest req)
    {
        var response = await _http.PostAsJsonAsync("productos", req);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> Actualizar(Guid id, CrearProductoRequest req)
    {
        var response = await _http.PutAsJsonAsync($"productos/{id}", req);
        return response.IsSuccessStatusCode;
    }

    public async Task<(bool exito, string? mensaje)> Eliminar(Guid id)
    {
        var response = await _http.DeleteAsync($"productos/{id}");
        if (response.IsSuccessStatusCode) return (true, null);

        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        return (false, body?.GetValueOrDefault("mensaje") ?? "No se pudo eliminar.");
    }
}