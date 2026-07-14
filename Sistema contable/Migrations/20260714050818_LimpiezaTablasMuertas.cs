using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sistema_contable.Migrations
{
    /// <inheritdoc />
    public partial class LimpiezaTablasMuertas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Backups");

            migrationBuilder.DropTable(
                name: "ConfiguracionSistema");

            migrationBuilder.DropTable(
                name: "CuentasContables");

            migrationBuilder.DropTable(
                name: "DetalleAsiento");

            migrationBuilder.DropTable(
                name: "Documentos");

            migrationBuilder.DropTable(
                name: "Empresas");

            migrationBuilder.DropTable(
                name: "EmpresasCliente");

            migrationBuilder.DropTable(
                name: "FacturasInternas");

            migrationBuilder.DropTable(
                name: "Monedas");

            migrationBuilder.DropTable(
                name: "PartidasReexpresion");

            migrationBuilder.DropTable(
                name: "RegistrosIpc");

            migrationBuilder.DropTable(
                name: "Asientos");

            migrationBuilder.AddColumn<string>(
                name: "TipoPago",
                table: "FacturasCobranza",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TipoPago",
                table: "FacturasCobranza");

            migrationBuilder.CreateTable(
                name: "Asientos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Descripcion = table.Column<string>(type: "TEXT", nullable: false),
                    EmpresaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Fecha = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Numero = table.Column<int>(type: "INTEGER", nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asientos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Backups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Fecha = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NombreArchivo = table.Column<string>(type: "TEXT", nullable: false),
                    RutaCompleta = table.Column<string>(type: "TEXT", nullable: false),
                    Tamaño = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Backups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConfiguracionSistema",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AutoguardadoHabilitado = table.Column<bool>(type: "INTEGER", nullable: false),
                    BackupAutomaticoHabilitado = table.Column<bool>(type: "INTEGER", nullable: false),
                    ConfirmarAntesDeEliminar = table.Column<bool>(type: "INTEGER", nullable: false),
                    EjercicioFiscal = table.Column<string>(type: "TEXT", nullable: false),
                    MonedaBase = table.Column<string>(type: "TEXT", nullable: false),
                    MostrarAlertasVencimientos = table.Column<bool>(type: "INTEGER", nullable: false),
                    PorcentajeIslr = table.Column<decimal>(type: "TEXT", nullable: false),
                    PorcentajeIva = table.Column<decimal>(type: "TEXT", nullable: false),
                    RegimenFiscal = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionSistema", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CuentasContables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AceptaMovimiento = table.Column<bool>(type: "INTEGER", nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    Codigo = table.Column<string>(type: "TEXT", nullable: false),
                    CuentaPadreId = table.Column<int>(type: "INTEGER", nullable: true),
                    EmpresaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Nivel = table.Column<int>(type: "INTEGER", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuentasContables", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Documentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Descripcion = table.Column<string>(type: "TEXT", nullable: false),
                    EmpresaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Estado = table.Column<string>(type: "TEXT", nullable: false),
                    FechaEntrega = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FechaRecepcion = table.Column<DateTime>(type: "TEXT", nullable: true),
                    NombreEmpresa = table.Column<string>(type: "TEXT", nullable: false),
                    Observaciones = table.Column<string>(type: "TEXT", nullable: false),
                    TipoDocumento = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documentos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Empresas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ActividadEconomica = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Direccion = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Rif = table.Column<string>(type: "TEXT", nullable: false),
                    Telefono = table.Column<string>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empresas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmpresasCliente",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Direccion = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    NombreEmpresa = table.Column<string>(type: "TEXT", nullable: false),
                    RazonSocial = table.Column<string>(type: "TEXT", nullable: false),
                    Rif = table.Column<string>(type: "TEXT", nullable: false),
                    Telefono = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmpresasCliente", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FacturasInternas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClienteId = table.Column<int>(type: "INTEGER", nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", nullable: false),
                    Estado = table.Column<string>(type: "TEXT", nullable: false),
                    FechaEmision = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaPago = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FechaVencimiento = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Monto = table.Column<decimal>(type: "TEXT", nullable: false),
                    NombreCliente = table.Column<string>(type: "TEXT", nullable: false),
                    NumeroFactura = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacturasInternas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Monedas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Codigo = table.Column<string>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    TasaCambio = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Monedas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PartidasReexpresion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Aplicar = table.Column<bool>(type: "INTEGER", nullable: false),
                    Codigo = table.Column<string>(type: "TEXT", nullable: false),
                    Diferencia = table.Column<decimal>(type: "TEXT", nullable: false),
                    Factor = table.Column<decimal>(type: "TEXT", nullable: false),
                    Moneda = table.Column<string>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", nullable: false),
                    ValorAjustado = table.Column<decimal>(type: "TEXT", nullable: false),
                    ValorOriginal = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartidasReexpresion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RegistrosIpc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Fecha = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Valor = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosIpc", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DetalleAsiento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AsientoId = table.Column<int>(type: "INTEGER", nullable: false),
                    CodigoCuenta = table.Column<string>(type: "TEXT", nullable: false),
                    CuentaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Debe = table.Column<decimal>(type: "TEXT", nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", nullable: false),
                    Haber = table.Column<decimal>(type: "TEXT", nullable: false),
                    NombreCuenta = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetalleAsiento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetalleAsiento_Asientos_AsientoId",
                        column: x => x.AsientoId,
                        principalTable: "Asientos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CuentasContables_EmpresaId_Codigo",
                table: "CuentasContables",
                columns: new[] { "EmpresaId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DetalleAsiento_AsientoId",
                table: "DetalleAsiento",
                column: "AsientoId");
        }
    }
}
