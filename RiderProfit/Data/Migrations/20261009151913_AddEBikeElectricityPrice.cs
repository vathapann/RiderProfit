using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RiderProfit.Migrations
{
    /// <inheritdoc />
    public partial class AddEBikeElectricityPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ElectricityPricePerKwh",
                table: "Vehicles",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ElectricityPricePerKwh",
                table: "Vehicles");
        }
    }
}
