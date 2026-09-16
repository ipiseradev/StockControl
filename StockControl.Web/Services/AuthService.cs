using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.JSInterop;

namespace StockControl.Web.Services;

public enum RolUsuario
{
    Administrador,
    Empleado
}

public record LoginRequest(string Email, string Password);
public record AuthResponse(string Token, string NombreUsuario, string NombreComercio, Guid TenantId, RolUsuario Rol);

public class AuthService
{
    private readonly HttpClient _http;
    private readonly IJSRuntime _js;

    public AuthResponse? UsuarioActual { get; private set; }
    public bool EsAdmin => UsuarioActual?.Rol == RolUsuario.Administrador;

    public AuthService(HttpClient http, IJSRuntime js)
    {
        _http = http;
        _js = js;
    }

    public async Task<bool> Login(string email, string password)
    {
        var response = await _http.PostAsJsonAsync("auth/login", new LoginRequest(email, password));
        if (!response.IsSuccessStatusCode)
            return false;

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions.Default);
        if (auth is null) return false;

        UsuarioActual = auth;
        await _js.InvokeVoidAsync("localStorage.setItem", "authToken", auth.Token);
        await _js.InvokeVoidAsync("localStorage.setItem", "authData", System.Text.Json.JsonSerializer.Serialize(auth, JsonOptions.Default));

        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);

        return true;
    }

    public async Task<bool> CargarSesionGuardada()
    {
        if (UsuarioActual is not null)
            return true;

        var token = await _js.InvokeAsync<string?>("localStorage.getItem", "authToken");
        var dataJson = await _js.InvokeAsync<string?>("localStorage.getItem", "authData");

        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(dataJson))
            return false;

        UsuarioActual = System.Text.Json.JsonSerializer.Deserialize<AuthResponse>(dataJson, JsonOptions.Default);
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return true;
    }

    public async Task Logout()
    {
        UsuarioActual = null;
        _http.DefaultRequestHeaders.Authorization = null;
        await _js.InvokeVoidAsync("localStorage.removeItem", "authToken");
        await _js.InvokeVoidAsync("localStorage.removeItem", "authData");
    }
}