namespace StockControl.Api.Models.Dtos;

public record CrearUsuarioRequest(string Nombre, string Email, string Password, RolUsuario Rol);
public record UsuarioResponse(Guid Id, string Nombre, string Email, RolUsuario Rol, DateTime FechaAlta);
