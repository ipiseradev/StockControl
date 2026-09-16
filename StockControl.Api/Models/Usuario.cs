namespace StockControl.Api.Models;

public enum RolUsuario
{
    Administrador,
    Empleado
}

public class Usuario
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }

    public Tenant Tenant { get; set; } = null!;

    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public RolUsuario Rol { get; set; } = RolUsuario.Empleado;

    public DateTime FechaAlta { get; set; } = DateTime.UtcNow;
}
