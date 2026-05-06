using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GardenTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRetiredDateFromBeds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RetiredDate",
                table: "RaisedBeds");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "RetiredDate",
                table: "RaisedBeds",
                type: "date",
                nullable: true);
        }
    }
}
