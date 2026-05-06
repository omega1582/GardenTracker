using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GardenTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameRaisedBedsToBeds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop FKs referencing RaisedBeds before renaming
            migrationBuilder.DropForeignKey(
                name: "FK_BedPlantings_RaisedBeds_BedId",
                table: "BedPlantings");

            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_RaisedBeds_BedId",
                table: "Expenses");

            migrationBuilder.DropForeignKey(
                name: "FK_Harvests_RaisedBeds_BedId",
                table: "Harvests");

            // Rename the table in-place — no data loss
            migrationBuilder.RenameTable(
                name: "RaisedBeds",
                newName: "Beds");

            migrationBuilder.RenameIndex(
                name: "IX_RaisedBeds_GardenId",
                table: "Beds",
                newName: "IX_Beds_GardenId");

            // Re-add FKs pointing at the renamed table
            migrationBuilder.AddForeignKey(
                name: "FK_BedPlantings_Beds_BedId",
                table: "BedPlantings",
                column: "BedId",
                principalTable: "Beds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_Beds_BedId",
                table: "Expenses",
                column: "BedId",
                principalTable: "Beds",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Harvests_Beds_BedId",
                table: "Harvests",
                column: "BedId",
                principalTable: "Beds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BedPlantings_Beds_BedId",
                table: "BedPlantings");

            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_Beds_BedId",
                table: "Expenses");

            migrationBuilder.DropForeignKey(
                name: "FK_Harvests_Beds_BedId",
                table: "Harvests");

            migrationBuilder.RenameIndex(
                name: "IX_Beds_GardenId",
                table: "Beds",
                newName: "IX_RaisedBeds_GardenId");

            migrationBuilder.RenameTable(
                name: "Beds",
                newName: "RaisedBeds");

            migrationBuilder.AddForeignKey(
                name: "FK_BedPlantings_RaisedBeds_BedId",
                table: "BedPlantings",
                column: "BedId",
                principalTable: "RaisedBeds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_RaisedBeds_BedId",
                table: "Expenses",
                column: "BedId",
                principalTable: "RaisedBeds",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Harvests_RaisedBeds_BedId",
                table: "Harvests",
                column: "BedId",
                principalTable: "RaisedBeds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
