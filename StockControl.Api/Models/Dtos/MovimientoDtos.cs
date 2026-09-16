namespace StockControl.Api.Models.Dtos;

public record CrearMovimientoRequest(Guid ProductoId, Guid DepositoId, TipoMovimiento Tipo, int Cantidad, string? Nota);
public record MovimientoResponse(Guid Id, Guid ProductoId, string ProductoNombre, Guid DepositoId, string DepositoNombre, TipoMovimiento Tipo, int Cantidad, DateTime Fecha, string? Nota);