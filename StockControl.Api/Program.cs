using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StockControl.Api.Data;
using StockControl.Api.Models;
using StockControl.Api.Models.Dtos;
using StockControl.Api.Services;
using System.Security.Claims;
using StockControl.Api.Extensions;
using StockControl.Api.Middleware;
using Microsoft.AspNetCore.SignalR;
using StockControl.Api.Hubs;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<TokenService>();

builder.Services.AddSignalR();

builder.Services.AddHttpClient<MercadoPagoService>();

// Configuración de autenticación JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SoloAdmin", policy => policy.RequireRole("Administrador"));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins("https://localhost:7127", "http://localhost:5249") // puertos reales del StockControl.Web (Properties/launchSettings.json)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseMiddleware<ManejadorErroresMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("FrontendPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<StockHub>("/hubs/stock");

// ENDPOINTS DE AUTH

app.MapPost("/auth/registro", async (RegistroRequest req, AppDbContext db, TokenService tokenService) =>
{
    if (await db.Usuarios.AnyAsync(u => u.Email == req.Email))
        return Results.BadRequest(new { mensaje = "Ya existe un usuario con ese email." });

    var tenant = new Tenant
    {
        NombreComercio = req.NombreComercio
    };

    var usuario = new Usuario
    {
        TenantId = tenant.Id,
        Tenant = tenant,
        Nombre = req.NombreUsuario,
        Email = req.Email,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
        Rol = RolUsuario.Administrador // el primer usuario del comercio es administrador
    };

    db.Tenants.Add(tenant);
    db.Usuarios.Add(usuario);
    await db.SaveChangesAsync();

    var token = tokenService.GenerarToken(usuario);

    return Results.Ok(new AuthResponse(token, usuario.Nombre, tenant.NombreComercio, tenant.Id, usuario.Rol));
});

app.MapPost("/auth/login", async (LoginRequest req, AppDbContext db, TokenService tokenService) =>
{
    var usuario = await db.Usuarios
        .Include(u => u.Tenant)
        .FirstOrDefaultAsync(u => u.Email == req.Email);

    if (usuario is null || !BCrypt.Net.BCrypt.Verify(req.Password, usuario.PasswordHash))
        return Results.Unauthorized();

    var token = tokenService.GenerarToken(usuario);

    return Results.Ok(new AuthResponse(token, usuario.Nombre, usuario.Tenant.NombreComercio, usuario.TenantId, usuario.Rol));
});


/// --- Endopints De Productos ----

var productos = app.MapGroup("/productos").RequireAuthorization();

productos.MapGet("/", async (ClaimsPrincipal user, AppDbContext db) =>
{
    var tenantId = user.GetTenantId();

    var lista = await db.Productos
        .Where(p => p.TenantId == tenantId)
        .Select(p => new ProductoResponse(
            p.Id, p.Nombre, p.CodigoBarras, p.Sku, p.PrecioVenta, p.StockMinimo,
            p.Movimientos.Sum(m => m.Tipo == TipoMovimiento.Entrada || m.Tipo == TipoMovimiento.Ajuste ? m.Cantidad : -m.Cantidad)
        ))
        .ToListAsync();

    return Results.Ok(lista);
});

productos.MapPost("/", async (CrearProductoRequest req, ClaimsPrincipal user, AppDbContext db) =>
{
    var tenantId = user.GetTenantId();

    var producto = new Producto
    {
        TenantId = tenantId,
        Nombre = req.Nombre,
        CodigoBarras = req.CodigoBarras,
        Sku = req.Sku,
        PrecioVenta = req.PrecioVenta,
        StockMinimo = req.StockMinimo
    };

    db.Productos.Add(producto);
    await db.SaveChangesAsync();

    return Results.Created($"/productos/{producto.Id}", new ProductoResponse(
        producto.Id, producto.Nombre, producto.CodigoBarras, producto.Sku,
        producto.PrecioVenta, producto.StockMinimo, 0));
});

productos.MapDelete("/{id}", async (Guid id, ClaimsPrincipal user, AppDbContext db) =>
{
    var tenantId = user.GetTenantId();

    var producto = await db.Productos.FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId);
    if (producto is null)
        return Results.NotFound(new { mensaje = "Producto no encontrado." });

    var tieneMovimientos = await db.Movimientos.AnyAsync(m => m.ProductoId == id);
    if (tieneMovimientos)
        return Results.BadRequest(new { mensaje = "No se puede eliminar un producto con movimientos registrados. Considerá desactivarlo en su lugar." });

    db.Productos.Remove(producto);
    await db.SaveChangesAsync();

    return Results.NoContent();
});

productos.MapPut("/{id}", async (Guid id, CrearProductoRequest req, ClaimsPrincipal user, AppDbContext db) =>
{
    var tenantId = user.GetTenantId();

    var producto = await db.Productos.FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId);
    if (producto is null)
        return Results.NotFound(new { mensaje = "Producto no encontrado." });

    producto.Nombre = req.Nombre;
    producto.CodigoBarras = req.CodigoBarras;
    producto.Sku = req.Sku;
    producto.PrecioVenta = req.PrecioVenta;
    producto.StockMinimo = req.StockMinimo;

    await db.SaveChangesAsync();

    var stockActual = await db.Movimientos
        .Where(m => m.ProductoId == id)
        .SumAsync(m => m.Tipo == TipoMovimiento.Entrada || m.Tipo == TipoMovimiento.Ajuste ? m.Cantidad : -m.Cantidad);

    return Results.Ok(new ProductoResponse(producto.Id, producto.Nombre, producto.CodigoBarras, producto.Sku, producto.PrecioVenta, producto.StockMinimo, stockActual));
});

productos.MapGet("/alertas", async (ClaimsPrincipal user, AppDbContext db) =>
{
    var tenantId = user.GetTenantId();

    var productosConStock = await db.Productos
        .Where(p => p.TenantId == tenantId)
        .Select(p => new ProductoResponse(
            p.Id, p.Nombre, p.CodigoBarras, p.Sku, p.PrecioVenta, p.StockMinimo,
            p.Movimientos.Sum(m => m.Tipo == TipoMovimiento.Entrada || m.Tipo == TipoMovimiento.Ajuste ? m.Cantidad : -m.Cantidad)
        ))
        .ToListAsync();

    var enAlerta = productosConStock
        .Where(p => p.StockActual <= p.StockMinimo)
        .ToList();

    return Results.Ok(enAlerta);
});

/// --- Endopins de Depositos ----

var depositos = app.MapGroup("/depositos").RequireAuthorization();

depositos.MapGet("/", async (ClaimsPrincipal user, AppDbContext db) =>
{
    var tenantId = user.GetTenantId();

    var lista = await db.Depositos
        .Where(d => d.TenantId == tenantId)
        .Select(d => new DepositoResponse(d.Id, d.Nombre, d.EsMostrador))
        .ToListAsync();

    return Results.Ok(lista);
});

depositos.MapPost("/", async (CrearDepositoRequest req, ClaimsPrincipal user, AppDbContext db) =>
{
    var tenantId = user.GetTenantId();

    var deposito = new Deposito
    {
        TenantId = tenantId,
        Nombre = req.Nombre,
        EsMostrador = req.EsMostrador
    };

    db.Depositos.Add(deposito);
    await db.SaveChangesAsync();

    return Results.Created($"/depositos/{deposito.Id}", new DepositoResponse(deposito.Id, deposito.Nombre, deposito.EsMostrador));
});

depositos.MapPut("/{id}", async (Guid id, CrearDepositoRequest req, ClaimsPrincipal user, AppDbContext db) =>
{
    var tenantId = user.GetTenantId();

    var deposito = await db.Depositos.FirstOrDefaultAsync(d => d.Id == id && d.TenantId == tenantId);
    if (deposito is null)
        return Results.NotFound(new { mensaje = "Depósito no encontrado." });

    deposito.Nombre = req.Nombre;
    deposito.EsMostrador = req.EsMostrador;

    await db.SaveChangesAsync();

    return Results.Ok(new DepositoResponse(deposito.Id, deposito.Nombre, deposito.EsMostrador));
});


// --- ENDPOINTS DE MOVIMIENTOS ---

var movimientos = app.MapGroup("/movimientos").RequireAuthorization();

movimientos.MapPost("/", async (CrearMovimientoRequest req, ClaimsPrincipal user, AppDbContext db, IHubContext<StockHub> hub) =>
{
    var tenantId = user.GetTenantId();
    var usuarioId = user.GetUsuarioId();

    // Verificamos que el producto y el depósito pertenezcan a este tenant
    // (evita que alguien cargue movimientos sobre datos de otro comercio)
    var producto = await db.Productos.FirstOrDefaultAsync(p => p.Id == req.ProductoId && p.TenantId == tenantId);
    if (producto is null)
        return Results.BadRequest(new { mensaje = "Producto no encontrado." });

    var deposito = await db.Depositos.FirstOrDefaultAsync(d => d.Id == req.DepositoId && d.TenantId == tenantId);
    if (deposito is null)
        return Results.BadRequest(new { mensaje = "Depósito no encontrado." });

    if (req.Cantidad <= 0)
        return Results.BadRequest(new { mensaje = "La cantidad debe ser mayor a cero." });

    var movimiento = new MovimientoStock
    {
        TenantId = tenantId,
        ProductoId = producto.Id,
        DepositoId = deposito.Id,
        UsuarioId = usuarioId,
        Tipo = req.Tipo,
        Cantidad = req.Cantidad,
        Nota = req.Nota
    };

    db.Movimientos.Add(movimiento);
    await db.SaveChangesAsync();

    // Calculamos el nuevo stock y avisamos a todos los clientes conectados de este tenant
    var stockActual = await db.Movimientos
        .Where(m => m.ProductoId == producto.Id)
        .SumAsync(m => m.Tipo == TipoMovimiento.Entrada || m.Tipo == TipoMovimiento.Ajuste ? m.Cantidad : -m.Cantidad);

    await hub.Clients.Group(tenantId.ToString()).SendAsync("StockActualizado", new
    {
        productoId = producto.Id,
        productoNombre = producto.Nombre,
        stockActual
    });

    return Results.Created($"/movimientos/{movimiento.Id}", new MovimientoResponse(
        movimiento.Id, producto.Id, producto.Nombre, deposito.Id, deposito.Nombre,
        movimiento.Tipo, movimiento.Cantidad, movimiento.Fecha, movimiento.Nota));
});

movimientos.MapGet("/", async (ClaimsPrincipal user, AppDbContext db) =>
{
    var tenantId = user.GetTenantId();

    var lista = await db.Movimientos
        .Where(m => m.TenantId == tenantId)
        .OrderByDescending(m => m.Fecha)
        .Select(m => new MovimientoResponse(
            m.Id, m.ProductoId, m.Producto.Nombre, m.DepositoId, m.Deposito.Nombre,
            m.Tipo, m.Cantidad, m.Fecha, m.Nota))
        .ToListAsync();

    return Results.Ok(lista);
});

// ---------- ENDPOINTS DE USUARIOS ----------

var usuarios = app.MapGroup("/usuarios").RequireAuthorization();

usuarios.MapGet("/", async (ClaimsPrincipal user, AppDbContext db) =>
{
    var tenantId = user.GetTenantId();

    var lista = await db.Usuarios
        .Where(u => u.TenantId == tenantId)
        .Select(u => new UsuarioResponse(u.Id, u.Nombre, u.Email, u.Rol, u.FechaAlta))
        .ToListAsync();

    return Results.Ok(lista);
});

usuarios.MapPost("/", async (CrearUsuarioRequest req, ClaimsPrincipal user, AppDbContext db) =>
{
    var tenantId = user.GetTenantId();

    if (await db.Usuarios.AnyAsync(u => u.Email == req.Email))
        return Results.BadRequest(new { mensaje = "Ya existe un usuario con ese email." });

    var nuevoUsuario = new Usuario
    {
        TenantId = tenantId,
        Nombre = req.Nombre,
        Email = req.Email,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
        Rol = req.Rol
    };

    db.Usuarios.Add(nuevoUsuario);
    await db.SaveChangesAsync();

    return Results.Created($"/usuarios/{nuevoUsuario.Id}", new UsuarioResponse(nuevoUsuario.Id, nuevoUsuario.Nombre, nuevoUsuario.Email, nuevoUsuario.Rol, nuevoUsuario.FechaAlta));
})
.RequireAuthorization("SoloAdmin"); // solo el admin del comercio puede invitar empleados

// ---------- ENDPOINTS DE SUSCRIPCIÓN ----------

app.MapGet("/suscripcion", async (ClaimsPrincipal user, AppDbContext db) =>
{
    var tenantId = user.GetTenantId();
    var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId);
    if (tenant is null)
        return Results.NotFound();

    return Results.Ok(new SuscripcionEstadoResponse(tenant.EstadoSuscripcion, tenant.FechaProximoPago));
})
.RequireAuthorization();

app.MapPost("/suscripcion/generar-link", async (CrearSuscripcionRequest req, ClaimsPrincipal user, AppDbContext db, MercadoPagoService mpService, IConfiguration config) =>
{
    var tenantId = user.GetTenantId();
    var usuarioId = user.GetUsuarioId();

    var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId);
    var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == usuarioId);

    if (tenant is null || usuario is null)
        return Results.NotFound();

    const decimal precioMensual = 9900m;
    var emailPagador = string.IsNullOrWhiteSpace(req.EmailComprador) ? usuario.Email : req.EmailComprador.Trim();

    // Mercado Pago exige una URL pública válida (no acepta localhost); en producción esto debe
    // apuntar al frontend real.
    var backUrl = config["MercadoPago:BackUrl"] ?? "https://www.mercadopago.com.ar";

    var (linkPago, preapprovalId) = await mpService.CrearSuscripcion(
        emailPagador, tenant.NombreComercio, precioMensual, backUrl);

    tenant.MercadoPagoPreapprovalId = preapprovalId;
    await db.SaveChangesAsync();

    return Results.Ok(new CrearSuscripcionResponse(linkPago, preapprovalId));
})
.RequireAuthorization("SoloAdmin");

// Mercado Pago llama esto directamente (sin JWT nuestro), por eso no lleva RequireAuthorization.
app.MapPost("/webhooks/mercadopago", async (MercadoPagoWebhookRequest req, AppDbContext db, MercadoPagoService mpService) =>
{
    if (req.Type != "subscription_preapproval" || string.IsNullOrWhiteSpace(req.Data?.Id))
        return Results.Ok();

    var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.MercadoPagoPreapprovalId == req.Data.Id);
    if (tenant is null)
        return Results.Ok();

    // No confiamos en el payload del webhook: consultamos el estado real contra la API de Mercado Pago.
    var estado = await mpService.ObtenerEstadoPreapproval(req.Data.Id);

    tenant.EstadoSuscripcion = estado switch
    {
        "authorized" => EstadoSuscripcion.Activa,
        "paused" => EstadoSuscripcion.Vencida,
        "cancelled" => EstadoSuscripcion.Cancelada,
        _ => tenant.EstadoSuscripcion
    };

    await db.SaveChangesAsync();

    return Results.Ok();
});

app.Run();