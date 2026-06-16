using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GardenTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddExtendedPlantAttributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DaysToMaturity",
                table: "PlantVarieties",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GrowthHabit",
                table: "PlantVarieties",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPerennial",
                table: "PlantVarieties",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SpacingInches",
                table: "PlantVarieties",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SunPreference",
                table: "PlantVarieties",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DaysToMaturity",
                table: "PlantTypes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GrowthHabit",
                table: "PlantTypes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPerennial",
                table: "PlantTypes",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SpacingInches",
                table: "PlantTypes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SunPreference",
                table: "PlantTypes",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DaysToMaturity",
                table: "PlantVarieties");

            migrationBuilder.DropColumn(
                name: "GrowthHabit",
                table: "PlantVarieties");

            migrationBuilder.DropColumn(
                name: "IsPerennial",
                table: "PlantVarieties");

            migrationBuilder.DropColumn(
                name: "SpacingInches",
                table: "PlantVarieties");

            migrationBuilder.DropColumn(
                name: "SunPreference",
                table: "PlantVarieties");

            migrationBuilder.DropColumn(
                name: "DaysToMaturity",
                table: "PlantTypes");

            migrationBuilder.DropColumn(
                name: "GrowthHabit",
                table: "PlantTypes");

            migrationBuilder.DropColumn(
                name: "IsPerennial",
                table: "PlantTypes");

            migrationBuilder.DropColumn(
                name: "SpacingInches",
                table: "PlantTypes");

            migrationBuilder.DropColumn(
                name: "SunPreference",
                table: "PlantTypes");
        }
    }
}
