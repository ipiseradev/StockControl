namespace StockControl.Api.Models;

public class Producto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public string Nombre { get; set; } = string.Empty;
    public string? CodigoBarras { get; set; }
    public string? Sku { get; set; }
    public decimal PrecioVenta { get; set; }
    public int StockMinimo { get; set; } = 0;

    public ICollection<MovimientoStock> Movimientos { get; set; } = new List<MovimientoStock>();
}