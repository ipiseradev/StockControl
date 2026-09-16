using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockControl.Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregarSuscripcion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EstadoSuscripcion",
                table: "Tenants",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaProximoPago",
                table: "Tenants",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MercadoPagoPreapprovalId",
                table: "Tenants",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstadoSuscripcion",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "FechaProximoPago",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "MercadoPagoPreapprovalId",
                table: "Tenants");
        }
    }
}
