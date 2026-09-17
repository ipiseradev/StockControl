using Microsoft.EntityFrameworkCore;
using StockControl.Api.Models;

namespace StockControl.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Deposito> Depositos => Set<Deposito>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<MovimientoStock> Movimientos => Set<MovimientoStock>();
    public DbSet<EventoAgenda> EventosAgenda => Set<EventoAgenda>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Índices únicos por tenant (dos comercios pueden tener el mismo email o SKU, pero no el mismo comercio dos veces)
        modelBuilder.Entity<Usuario>()
            .HasIndex(u => new { u.TenantId, u.Email })
            .IsUnique();

        modelBuilder.Entity<Producto>()
            .HasIndex(p => new { p.TenantId, p.Sku })
            .IsUnique()
            .HasFilter("\"Sku\" IS NOT NULL");
    }
}