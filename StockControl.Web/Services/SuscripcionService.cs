using System.Net.Http.Json;

namespace StockControl.Web.Services;

public enum EstadoSuscripcion
{
    Trial,
    Activa,
    Vencida,
    Cancelada
}

public record SuscripcionEstadoResponse(EstadoSuscripcion EstadoSuscripcion, DateTime? FechaProximoPago);
public record CrearSuscripcionRequest(string? EmailComprador);
public record CrearSuscripcionResponse(string LinkPago, string PreapprovalId);

public class SuscripcionService
{
    private readonly HttpClient _http;

    public SuscripcionService(HttpClient http)
    {
        _http = http;
    }

    public async Task<SuscripcionEstadoResponse?> ObtenerEstado()
    {
        return await _http.GetFromJsonAsync<SuscripcionEstadoResponse>("suscripcion", JsonOptions.Default);
    }

    public async Task<CrearSuscripcionResponse?> GenerarLink(string? emailComprador)
    {
        var response = await _http.PostAsJsonAsync("suscripcion/generar-link", new CrearSuscripcionRequest(emailComprador), JsonOptions.Default);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<CrearSuscripcionResponse>(JsonOptions.Default);
    }
}
