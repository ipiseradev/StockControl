using System.Security.Claims;

namespace StockControl.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetTenantId(this ClaimsPrincipal user)
    {
        var value = user.FindFirst("tenantId")?.Value
            ?? throw new InvalidOperationException("El token no contiene tenantId.");
        return Guid.Parse(value);
    }

    public static Guid GetUsuarioId(this ClaimsPrincipal user)
    {
        var value = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new InvalidOperationException("El token no contiene el id del usuario.");
        return Guid.Parse(value);
    }
}