using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockControl.Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregarAgenda : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventosAgenda",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Titulo = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    Hora = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    Completada = table.Column<bool>(type: "boolean", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventosAgenda", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventosAgenda_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventosAgenda_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventosAgenda_TenantId",
                table: "EventosAgenda",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_EventosAgenda_UsuarioId",
                table: "EventosAgenda",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventosAgenda");
        }
    }
}
