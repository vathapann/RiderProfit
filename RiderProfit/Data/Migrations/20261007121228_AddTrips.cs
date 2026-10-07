using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RiderProfit.Migrations
{
    /// <inheritdoc />
    public partial class AddTrips : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DistanceKm",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "Earnings",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "Suburb",
                table: "Shifts");

            migrationBuilder.RenameColumn(
                name: "Tips",
                table: "Shifts",
                newName: "TotalDistanceKm");

            migrationBuilder.CreateTable(
                name: "Trips",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ShiftId = table.Column<int>(type: "INTEGER", nullable: false),
                    Suburb = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Earnings = table.Column<decimal>(type: "TEXT", nullable: false),
                    DistanceKm = table.Column<decimal>(type: "TEXT", nullable: false),
                    Tips = table.Column<decimal>(type: "TEXT", nullable: false),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PickupName = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trips_Shifts_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "Shifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Trips_ShiftId",
                table: "Trips",
                column: "ShiftId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Trips");

            migrationBuilder.RenameColumn(
                name: "TotalDistanceKm",
                table: "Shifts",
                newName: "Tips");

            migrationBuilder.AddColumn<decimal>(
                name: "DistanceKm",
                table: "Shifts",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Earnings",
                table: "Shifts",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Suburb",
                table: "Shifts",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
