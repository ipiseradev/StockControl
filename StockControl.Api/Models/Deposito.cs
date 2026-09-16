namespace StockControl.Api.Models;

public class Deposito
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public string Nombre { get; set; } = string.Empty; // ej: "Mostrador", "Depósito principal"
    public bool EsMostrador { get; set; } = false;

    public ICollection<MovimientoStock> Movimientos { get; set; } = new List<MovimientoStock>();
}