using System.Globalization;
using CsvHelper;
using FluentAssertions;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Enums;
using GardenTracker.Core.Interfaces.Repositories;
using GardenTracker.Services;
using Moq;

namespace GardenTracker.Tests.Services;

public class CsvExportServiceTests
{
    private readonly Mock<IInventoryRepository> _repo = new();
    private readonly CsvExportService _sut;

    public CsvExportServiceTests() => _sut = new CsvExportService(_repo.Object);

    [Fact]
    public async Task ExportInventoryAsync_EmptyInventory_ReturnsHeaderOnlyBytes()
    {
        _repo.Setup(r => r.GetByUserAsync(1)).ReturnsAsync([]);

        var bytes = await _sut.ExportInventoryAsync(1);
        var csv = System.Text.Encoding.UTF8.GetString(bytes);

        csv.Should().Contain("PlantTypeName");
        csv.Should().Contain("PlantVarietyName");
        csv.Should().Contain("QuantityPurchased");
        csv.Should().Contain("TotalCost");
    }

    [Fact]
    public async Task ExportInventoryAsync_WithItems_WritesAllFields()
    {
        var items = new List<InventoryItem>
        {
            new()
            {
                Id = 1, UserId = 42, PlantVarietyId = 10,
                PlantTypeName = "Tomato", PlantVarietyName = "Cherokee Purple",
                SupplierName = "Baker Creek",
                Type = InventoryType.Seed,
                QuantityPurchased = 50, QuantityRemaining = 30,
                TotalCost = 4.50m,
                PurchaseDate = new DateOnly(2025, 3, 15),
                Notes = "Great heirloom"
            }
        };
        _repo.Setup(r => r.GetByUserAsync(42)).ReturnsAsync(items);

        var bytes = await _sut.ExportInventoryAsync(42);
        var csv = System.Text.Encoding.UTF8.GetString(bytes);

        csv.Should().Contain("Tomato");
        csv.Should().Contain("Cherokee Purple");
        csv.Should().Contain("Baker Creek");
        csv.Should().Contain("Seed");
        csv.Should().Contain("50");
        csv.Should().Contain("30");
        csv.Should().Contain("4.5");
        csv.Should().Contain("2025-03-15");
        csv.Should().Contain("Great heirloom");
    }

    [Fact]
    public async Task ExportInventoryAsync_WithMultipleItems_WritesCorrectRowCount()
    {
        var items = Enumerable.Range(1, 5).Select(i => new InventoryItem
        {
            Id = i, UserId = 1, PlantVarietyId = i,
            PlantTypeName = $"Type{i}", PlantVarietyName = $"Variety{i}",
            Type = InventoryType.Seed,
            QuantityPurchased = 10, QuantityRemaining = 10,
            TotalCost = 2.00m,
            PurchaseDate = new DateOnly(2025, 1, i)
        }).ToList();
        _repo.Setup(r => r.GetByUserAsync(1)).ReturnsAsync(items);

        var bytes = await _sut.ExportInventoryAsync(1);
        var csv = System.Text.Encoding.UTF8.GetString(bytes);

        using var reader = new StringReader(csv);
        using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
        var rows = csvReader.GetRecords<dynamic>().ToList();
        rows.Should().HaveCount(5);
    }

    [Fact]
    public async Task ExportInventoryAsync_NullOptionalFields_WritesEmptyForOptional()
    {
        var items = new List<InventoryItem>
        {
            new()
            {
                Id = 1, UserId = 1, PlantVarietyId = 1,
                PlantTypeName = "Pepper", PlantVarietyName = "Jalapeño",
                SupplierName = null, Notes = null,
                Type = InventoryType.Plant,
                QuantityPurchased = 6, QuantityRemaining = 6,
                TotalCost = 12.00m,
                PurchaseDate = new DateOnly(2025, 5, 1)
            }
        };
        _repo.Setup(r => r.GetByUserAsync(1)).ReturnsAsync(items);

        var bytes = await _sut.ExportInventoryAsync(1);
        var csv = System.Text.Encoding.UTF8.GetString(bytes);

        csv.Should().NotBeNullOrWhiteSpace();
        csv.Should().Contain("Pepper");
        csv.Should().Contain("Plant");
    }
}
