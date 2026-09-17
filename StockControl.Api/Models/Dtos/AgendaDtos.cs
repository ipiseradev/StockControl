namespace StockControl.Api.Models.Dtos;

public record CrearEventoAgendaRequest(string Titulo, string? Descripcion, DateOnly Fecha, TimeOnly? Hora);
public record ActualizarEventoAgendaRequest(string Titulo, string? Descripcion, DateOnly Fecha, TimeOnly? Hora, bool Completada);
public record EventoAgendaResponse(Guid Id, string Titulo, string? Descripcion, DateOnly Fecha, TimeOnly? Hora, bool Completada, string UsuarioNombre);
