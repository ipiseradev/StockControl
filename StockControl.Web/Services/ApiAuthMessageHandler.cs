using System.Net;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace StockControl.Web.Services;

public class ApiAuthMessageHandler : DelegatingHandler
{
    private readonly NavigationManager _nav;
    private readonly IJSRuntime _js;

    public ApiAuthMessageHandler(NavigationManager nav, IJSRuntime js)
    {
        _nav = nav;
        _js = js;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized && !request.RequestUri!.AbsolutePath.EndsWith("/auth/login"))
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", "authToken");
            await _js.InvokeVoidAsync("localStorage.removeItem", "authData");
            _nav.NavigateTo("/login", forceLoad: true);
        }

        return response;
    }
}
