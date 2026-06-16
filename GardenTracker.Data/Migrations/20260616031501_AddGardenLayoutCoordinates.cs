using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GardenTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGardenLayoutCoordinates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PositionX",
                table: "Beds",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PositionY",
                table: "Beds",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LayoutHeight",
                table: "BedPlantings",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LayoutWidth",
                table: "BedPlantings",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PositionX",
                table: "BedPlantings",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PositionY",
                table: "BedPlantings",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PositionX",
                table: "Beds");

            migrationBuilder.DropColumn(
                name: "PositionY",
                table: "Beds");

            migrationBuilder.DropColumn(
                name: "LayoutHeight",
                table: "BedPlantings");

            migrationBuilder.DropColumn(
                name: "LayoutWidth",
                table: "BedPlantings");

            migrationBuilder.DropColumn(
                name: "PositionX",
                table: "BedPlantings");

            migrationBuilder.DropColumn(
                name: "PositionY",
                table: "BedPlantings");
        }
    }
}
