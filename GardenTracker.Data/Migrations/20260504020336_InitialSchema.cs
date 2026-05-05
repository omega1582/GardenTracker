using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GardenTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlantTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SupplierType = table.Column<int>(type: "int", nullable: false),
                    Website = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlantVarieties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlantTypeId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantVarieties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlantVarieties_PlantTypes_PlantTypeId",
                        column: x => x.PlantTypeId,
                        principalTable: "PlantTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Gardens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gardens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Gardens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RaisedBeds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GardenId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LengthFt = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    WidthFt = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    DepthIn = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    Material = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ExpectedLifespanYears = table.Column<int>(type: "int", nullable: false),
                    InstalledDate = table.Column<DateOnly>(type: "date", nullable: false),
                    RetiredDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaisedBeds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RaisedBeds_Gardens_GardenId",
                        column: x => x.GardenId,
                        principalTable: "Gardens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Seasons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GardenId = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seasons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Seasons_Gardens_GardenId",
                        column: x => x.GardenId,
                        principalTable: "Gardens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Expenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SeasonId = table.Column<int>(type: "int", nullable: false),
                    BedId = table.Column<int>(type: "int", nullable: true),
                    SupplierId = table.Column<int>(type: "int", nullable: true),
                    Category = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    ExpenseDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Expenses_RaisedBeds_BedId",
                        column: x => x.BedId,
                        principalTable: "RaisedBeds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Expenses_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Expenses_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Harvests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BedId = table.Column<int>(type: "int", nullable: false),
                    SeasonId = table.Column<int>(type: "int", nullable: false),
                    PlantVarietyId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(10,3)", precision: 10, scale: 3, nullable: false),
                    Unit = table.Column<int>(type: "int", nullable: false),
                    HarvestDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Harvests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Harvests_PlantVarieties_PlantVarietyId",
                        column: x => x.PlantVarietyId,
                        principalTable: "PlantVarieties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Harvests_RaisedBeds_BedId",
                        column: x => x.BedId,
                        principalTable: "RaisedBeds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Harvests_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MarketPrices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SeasonId = table.Column<int>(type: "int", nullable: false),
                    PlantTypeId = table.Column<int>(type: "int", nullable: false),
                    PlantVarietyId = table.Column<int>(type: "int", nullable: true),
                    PricePerUnit = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Unit = table.Column<int>(type: "int", nullable: false),
                    RecordedDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketPrices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarketPrices_PlantTypes_PlantTypeId",
                        column: x => x.PlantTypeId,
                        principalTable: "PlantTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MarketPrices_PlantVarieties_PlantVarietyId",
                        column: x => x.PlantVarietyId,
                        principalTable: "PlantVarieties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MarketPrices_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BedPlantings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BedId = table.Column<int>(type: "int", nullable: false),
                    SeasonId = table.Column<int>(type: "int", nullable: false),
                    PlantVarietyId = table.Column<int>(type: "int", nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: true),
                    StartMethod = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    TotalCost = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    SourceHarvestId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BedPlantings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BedPlantings_Harvests_SourceHarvestId",
                        column: x => x.SourceHarvestId,
                        principalTable: "Harvests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_BedPlantings_PlantVarieties_PlantVarietyId",
                        column: x => x.PlantVarietyId,
                        principalTable: "PlantVarieties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BedPlantings_RaisedBeds_BedId",
                        column: x => x.BedId,
                        principalTable: "RaisedBeds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BedPlantings_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BedPlantings_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BedPlantings_BedId",
                table: "BedPlantings",
                column: "BedId");

            migrationBuilder.CreateIndex(
                name: "IX_BedPlantings_PlantVarietyId",
                table: "BedPlantings",
                column: "PlantVarietyId");

            migrationBuilder.CreateIndex(
                name: "IX_BedPlantings_SeasonId",
                table: "BedPlantings",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_BedPlantings_SourceHarvestId",
                table: "BedPlantings",
                column: "SourceHarvestId");

            migrationBuilder.CreateIndex(
                name: "IX_BedPlantings_SupplierId",
                table: "BedPlantings",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_BedId",
                table: "Expenses",
                column: "BedId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_SeasonId",
                table: "Expenses",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_SupplierId",
                table: "Expenses",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_Gardens_UserId",
                table: "Gardens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Harvests_BedId",
                table: "Harvests",
                column: "BedId");

            migrationBuilder.CreateIndex(
                name: "IX_Harvests_PlantVarietyId",
                table: "Harvests",
                column: "PlantVarietyId");

            migrationBuilder.CreateIndex(
                name: "IX_Harvests_SeasonId",
                table: "Harvests",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketPrices_PlantTypeId",
                table: "MarketPrices",
                column: "PlantTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketPrices_PlantVarietyId",
                table: "MarketPrices",
                column: "PlantVarietyId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketPrices_SeasonId",
                table: "MarketPrices",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_PlantTypes_Name",
                table: "PlantTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlantVarieties_PlantTypeId_Name",
                table: "PlantVarieties",
                columns: new[] { "PlantTypeId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RaisedBeds_GardenId",
                table: "RaisedBeds",
                column: "GardenId");

            migrationBuilder.CreateIndex(
                name: "IX_Seasons_GardenId_Year",
                table: "Seasons",
                columns: new[] { "GardenId", "Year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BedPlantings");

            migrationBuilder.DropTable(
                name: "Expenses");

            migrationBuilder.DropTable(
                name: "MarketPrices");

            migrationBuilder.DropTable(
                name: "Harvests");

            migrationBuilder.DropTable(
                name: "Suppliers");

            migrationBuilder.DropTable(
                name: "PlantVarieties");

            migrationBuilder.DropTable(
                name: "RaisedBeds");

            migrationBuilder.DropTable(
                name: "Seasons");

            migrationBuilder.DropTable(
                name: "PlantTypes");

            migrationBuilder.DropTable(
                name: "Gardens");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
