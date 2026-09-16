namespace StockControl.Api.Models.Dtos;

public record CrearDepositoRequest(string Nombre, bool EsMostrador);
public record DepositoResponse(Guid Id, string Nombre, bool EsMostrador);