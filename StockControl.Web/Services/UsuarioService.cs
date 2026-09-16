using System.Net.Http.Json;

namespace StockControl.Web.Services;

public record Usuario(Guid Id, string Nombre, string Email, RolUsuario Rol, DateTime FechaAlta);
public record CrearUsuarioRequest(string Nombre, string Email, string Password, RolUsuario Rol);

public class UsuarioService
{
    private readonly HttpClient _http;

    public UsuarioService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<Usuario>> ObtenerTodos()
    {
        return await _http.GetFromJsonAsync<List<Usuario>>("usuarios", JsonOptions.Default) ?? new();
    }

    public async Task<(bool exito, string? mensaje)> Crear(CrearUsuarioRequest req)
    {
        var response = await _http.PostAsJsonAsync("usuarios", req, JsonOptions.Default);
        if (response.IsSuccessStatusCode) return (true, null);

        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        return (false, body?.GetValueOrDefault("mensaje") ?? "No se pudo crear el usuario.");
    }
}
