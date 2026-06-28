using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GardenTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddImageUrlToPlantVarieties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "PlantVarieties",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "PlantVarieties");
        }
    }
}
