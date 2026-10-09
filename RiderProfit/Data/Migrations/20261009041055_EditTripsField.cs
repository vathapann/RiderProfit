using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RiderProfit.Migrations
{
    /// <inheritdoc />
    public partial class EditTripsField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalDistanceKm",
                table: "Shifts");

            migrationBuilder.RenameColumn(
                name: "Suburb",
                table: "Trips",
                newName: "PickupSuburb");

            migrationBuilder.AddColumn<string>(
                name: "DropoffName",
                table: "Trips",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DropoffSuburb",
                table: "Trips",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsPublicHoliday",
                table: "Trips",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRaining",
                table: "Trips",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DropoffName",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "DropoffSuburb",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "IsPublicHoliday",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "IsRaining",
                table: "Trips");

            migrationBuilder.RenameColumn(
                name: "PickupSuburb",
                table: "Trips",
                newName: "Suburb");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalDistanceKm",
                table: "Shifts",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
