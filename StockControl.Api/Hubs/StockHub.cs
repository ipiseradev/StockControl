using Microsoft.AspNetCore.SignalR;

namespace StockControl.Api.Hubs;

public class StockHub : Hub
{
    // El cliente (Blazor) se conecta y se "suscribe" a su propio tenant,
    // así solo recibe notificaciones de su comercio, no de todos los comercios del sistema.
    public async Task UnirseATenant(string tenantId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, tenantId);
    }
}
