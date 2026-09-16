using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace StockControl.Api.Services;

public class MercadoPagoService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    public MercadoPagoService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;
        _http.BaseAddress = new Uri("https://api.mercadopago.com/");
    }

    public async Task<(string LinkPago, string PreapprovalId)> CrearSuscripcion(
        string emailPagador, string nombreComercio, decimal monto, string backUrl)
    {
        var accessToken = _config["MercadoPago:AccessToken"];

        var body = new
        {
            reason = $"Suscripción StockControl - {nombreComercio}",
            auto_recurring = new
            {
                frequency = 1,
                frequency_type = "months",
                transaction_amount = monto,
                currency_id = "ARS"
            },
            back_url = backUrl,
            payer_email = emailPagador,
            status = "pending"
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "preapproval")
        {
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _http.SendAsync(request);
        var contenido = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Error de Mercado Pago: {contenido}");

        using var doc = JsonDocument.Parse(contenido);
        var linkPago = doc.RootElement.GetProperty("init_point").GetString()!;
        var preapprovalId = doc.RootElement.GetProperty("id").GetString()!;

        return (linkPago, preapprovalId);
    }

    public async Task<string> ObtenerEstadoPreapproval(string preapprovalId)
    {
        var accessToken = _config["MercadoPago:AccessToken"];

        var request = new HttpRequestMessage(HttpMethod.Get, $"preapproval/{preapprovalId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _http.SendAsync(request);
        var contenido = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Error de Mercado Pago: {contenido}");

        using var doc = JsonDocument.Parse(contenido);
        return doc.RootElement.GetProperty("status").GetString()!;
    }
}
