using System.Net.Http.Json;

namespace StockControl.Web.Services;

public record EventoAgenda(Guid Id, string Titulo, string? Descripcion, DateOnly Fecha, TimeOnly? Hora, bool Completada, string UsuarioNombre);
public record CrearEventoAgendaRequest(string Titulo, string? Descripcion, DateOnly Fecha, TimeOnly? Hora);
public record ActualizarEventoAgendaRequest(string Titulo, string? Descripcion, DateOnly Fecha, TimeOnly? Hora, bool Completada);

public class AgendaService
{
    private readonly HttpClient _http;

    public AgendaService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<EventoAgenda>> ObtenerPorMes(int anio, int mes)
    {
        return await _http.GetFromJsonAsync<List<EventoAgenda>>($"agenda?anio={anio}&mes={mes}") ?? new();
    }

    public async Task<bool> Crear(CrearEventoAgendaRequest req)
    {
        var response = await _http.PostAsJsonAsync("agenda", req);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> Actualizar(Guid id, ActualizarEventoAgendaRequest req)
    {
        var response = await _http.PutAsJsonAsync($"agenda/{id}", req);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> Eliminar(Guid id)
    {
        var response = await _http.DeleteAsync($"agenda/{id}");
        return response.IsSuccessStatusCode;
    }
}
