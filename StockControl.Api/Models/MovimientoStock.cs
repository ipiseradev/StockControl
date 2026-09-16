namespace StockControl.Api.Models;

public enum TipoMovimiento
{
    Entrada,
    Salida,
    Ajuste,
    Merma
}

public class MovimientoStock
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public Guid ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;

    public Guid DepositoId { get; set; }
    public Deposito Deposito { get; set; } = null!;

    public TipoMovimiento Tipo { get; set; }
    public int Cantidad { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    public Guid UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public string? Nota { get; set; }
}