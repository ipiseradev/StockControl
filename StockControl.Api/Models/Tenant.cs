namespace StockControl.Api.Models;

public enum EstadoSuscripcion
{
    Trial,
    Activa,
    Vencida,
    Cancelada
}

public class Tenant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string NombreComercio { get; set; } = string.Empty;
    public DateTime FechaAlta { get; set; } = DateTime.UtcNow;
    public bool Activo { get; set; } = true;

    // Suscripción
    public EstadoSuscripcion EstadoSuscripcion { get; set; } = EstadoSuscripcion.Trial;
    public string? MercadoPagoPreapprovalId { get; set; }
    public DateTime? FechaProximoPago { get; set; }

    // Relaciones
    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    public ICollection<Deposito> Depositos { get; set; } = new List<Deposito>();
    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
