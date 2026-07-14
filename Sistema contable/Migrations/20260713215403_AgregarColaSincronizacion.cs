using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sistema_contable.Migrations
{
    /// <inheritdoc />
    public partial class AgregarColaSincronizacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Sincronizado",
                table: "FacturasCobranza",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "SyncId",
                table: "FacturasCobranza",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "Sincronizado",
                table: "ComprobantesContables",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "SyncId",
                table: "ComprobantesContables",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "ColaSincronizacion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TipoEntidad = table.Column<string>(type: "TEXT", nullable: false),
                    PayloadJson = table.Column<string>(type: "TEXT", nullable: false),
                    Intentos = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ColaSincronizacion", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ColaSincronizacion");

            migrationBuilder.DropColumn(
                name: "Sincronizado",
                table: "FacturasCobranza");

            migrationBuilder.DropColumn(
                name: "SyncId",
                table: "FacturasCobranza");

            migrationBuilder.DropColumn(
                name: "Sincronizado",
                table: "ComprobantesContables");

            migrationBuilder.DropColumn(
                name: "SyncId",
                table: "ComprobantesContables");
        }
    }
}
