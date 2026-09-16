using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockControl.Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregarUniqueSku : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Productos_TenantId_Sku",
                table: "Productos");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_TenantId_Sku",
                table: "Productos",
                columns: new[] { "TenantId", "Sku" },
                unique: true,
                filter: "\"Sku\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Productos_TenantId_Sku",
                table: "Productos");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_TenantId_Sku",
                table: "Productos",
                columns: new[] { "TenantId", "Sku" });
        }
    }
}
