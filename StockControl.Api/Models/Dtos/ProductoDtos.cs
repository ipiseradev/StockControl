namespace StockControl.Api.Models.Dtos;

public record CrearProductoRequest(string Nombre, string? CodigoBarras, string? Sku, decimal PrecioVenta, int StockMinimo);
public record ProductoResponse(Guid Id, string Nombre, string? CodigoBarras, string? Sku, decimal PrecioVenta, int StockMinimo, int StockActual);