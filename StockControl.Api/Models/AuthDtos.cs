namespace StockControl.Api.Models.Dtos;

public record RegistroRequest(string NombreComercio, string NombreUsuario, string Email, string Password);
public record LoginRequest(string Email, string Password);
public record AuthResponse(string Token, string NombreUsuario, string NombreComercio, Guid TenantId, RolUsuario Rol);