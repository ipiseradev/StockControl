using Microsoft.AspNetCore.SignalR.Client;

namespace StockControl.Web.Services;

public record StockUpdate(Guid ProductoId, string ProductoNombre, int StockActual);

public class RealtimeService : IAsyncDisposable
{
    private HubConnection? _connection;

    public event Action<StockUpdate>? StockActualizado;

    public async Task ConectarAsync(Guid tenantId)
    {
        if (_connection is not null)
            return;

        _connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5045/hubs/stock")
            .WithAutomaticReconnect()
            .Build();

        _connection.On<StockUpdate>("StockActualizado", update => StockActualizado?.Invoke(update));

        _connection.Reconnected += async _ =>
        {
            await _connection.InvokeAsync("UnirseATenant", tenantId.ToString());
        };

        await _connection.StartAsync();
        await _connection.InvokeAsync("UnirseATenant", tenantId.ToString());
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
            _connection = null;
        }
    }
}
