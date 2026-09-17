using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using StockControl.Web;
using StockControl.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddTransient<ApiAuthMessageHandler>();
builder.Services.AddHttpClient("StockControlAPI", client => client.BaseAddress = new Uri("https://localhost:7292/"))
    .AddHttpMessageHandler<ApiAuthMessageHandler>();
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("StockControlAPI"));

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ProductoService>();
builder.Services.AddScoped<DepositoService>();
builder.Services.AddScoped<MovimientoService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<SuscripcionService>();
builder.Services.AddScoped<AgendaService>();
builder.Services.AddScoped<RealtimeService>();

await builder.Build().RunAsync();