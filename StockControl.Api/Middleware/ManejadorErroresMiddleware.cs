using System.Net;
using System.Text.Json;

namespace StockControl.Api.Middleware;

public class ManejadorErroresMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ManejadorErroresMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ManejadorErroresMiddleware(RequestDelegate next, ILogger<ManejadorErroresMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado procesando {Path}", context.Request.Path);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            string mensaje = _env.IsDevelopment() ? "Ocurrió un error interno." : "Ocurrió un error interno. Intentá de nuevo más tarde.";
            string? detalle = _env.IsDevelopment() ? ex.Message : null;
            string? stackTrace = _env.IsDevelopment() ? ex.StackTrace : null;

            await context.Response.WriteAsync(JsonSerializer.Serialize(new { mensaje, detalle, stackTrace }));
        }
    }
}
