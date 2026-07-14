using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sistema_contable.Migrations
{
    /// <inheritdoc />
    public partial class AgregarIdComprobanteReversion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "IdComprobantePago",
                table: "FacturasCobranza",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<int>(
                name: "IdComprobanteReversion",
                table: "FacturasCobranza",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdComprobanteReversion",
                table: "FacturasCobranza");

            migrationBuilder.AlterColumn<int>(
                name: "IdComprobantePago",
                table: "FacturasCobranza",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);
        }
    }
}
