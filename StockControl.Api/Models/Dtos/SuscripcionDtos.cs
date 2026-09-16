namespace StockControl.Api.Models.Dtos;

public record CrearSuscripcionRequest(string? EmailComprador);
public record CrearSuscripcionResponse(string LinkPago, string PreapprovalId);
public record SuscripcionEstadoResponse(EstadoSuscripcion EstadoSuscripcion, DateTime? FechaProximoPago);

public record MercadoPagoWebhookRequest(string? Type, MercadoPagoWebhookData? Data);
public record MercadoPagoWebhookData(string? Id);
