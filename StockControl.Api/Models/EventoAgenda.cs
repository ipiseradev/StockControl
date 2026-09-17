namespace StockControl.Api.Models;

public class EventoAgenda
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }

    public DateOnly Fecha { get; set; }

    // Null = tarea/recordatorio sin horario fijo. Con valor = evento con hora puntual.
    public TimeOnly? Hora { get; set; }

    public bool Completada { get; set; } = false;

    public Guid UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
