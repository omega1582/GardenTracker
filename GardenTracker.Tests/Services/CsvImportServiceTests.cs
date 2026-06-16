using System.Text;
using FluentAssertions;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Enums;
using GardenTracker.Core.Interfaces.Repositories;
using GardenTracker.Services;
using Moq;

namespace GardenTracker.Tests.Services;

public class CsvImportServiceTests
{
    private readonly Mock<IInventoryRepository> _inventoryRepo = new();
    private readonly Mock<IPlantTypeRepository> _plantTypeRepo = new();
    private readonly Mock<IPlantVarietyRepository> _varietyRepo = new();
    private readonly Mock<ISupplierRepository> _supplierRepo = new();
    private readonly CsvImportService _sut;

    public CsvImportServiceTests()
    {
        _sut = new CsvImportService(
            _inventoryRepo.Object,
            _plantTypeRepo.Object,
            _varietyRepo.Object,
            _supplierRepo.Object);

        // Default: no existing items
        _inventoryRepo.Setup(r => r.GetByUserAsync(It.IsAny<int>())).ReturnsAsync([]);
        _inventoryRepo.Setup(r => r.CreateAsync(It.IsAny<InventoryItem>())).ReturnsAsync(1);
    }

    private static Stream MakeCsv(string content) =>
        new MemoryStream(Encoding.UTF8.GetBytes(content));

    [Fact]
    public async Task ImportInventoryAsync_ValidRow_CreatesPlantTypeVarietyAndItem()
    {
        const string csv = """
            PlantTypeName,PlantVarietyName,Type,QuantityPurchased,QuantityRemaining,TotalCost,PurchaseDate,SupplierName,SupplierType,Notes
            Tomato,Cherokee Purple,Seed,50,50,4.50,2025-03-15,,,
            """;

        _plantTypeRepo.Setup(r => r.GetByNameAsync("Tomato")).ReturnsAsync((PlantType?)null);
        _plantTypeRepo.Setup(r => r.CreateAsync(It.IsAny<PlantType>())).ReturnsAsync(1);
        _varietyRepo.Setup(r => r.GetByPlantTypeAndNameAsync(1, "Cherokee Purple")).ReturnsAsync((PlantVariety?)null);
        _varietyRepo.Setup(r => r.CreateAsync(It.IsAny<PlantVariety>())).ReturnsAsync(10);

        var result = await _sut.ImportInventoryAsync(42, MakeCsv(csv));

        result.Created.Should().Be(1);
        result.Updated.Should().Be(0);
        result.Errors.Should().BeEmpty();
        _plantTypeRepo.Verify(r => r.CreateAsync(It.Is<PlantType>(p => p.Name == "Tomato")), Times.Once);
        _varietyRepo.Verify(r => r.CreateAsync(It.Is<PlantVariety>(v => v.Name == "Cherokee Purple" && v.PlantTypeId == 1)), Times.Once);
        _inventoryRepo.Verify(r => r.CreateAsync(It.Is<InventoryItem>(i =>
            i.UserId == 42 &&
            i.PlantVarietyId == 10 &&
            i.Type == InventoryType.Seed &&
            i.QuantityPurchased == 50 &&
            i.TotalCost == 4.50m)), Times.Once);
    }

    [Fact]
    public async Task ImportInventoryAsync_ExistingPlantTypeAndVariety_DoesNotCreateDuplicates()
    {
        const string csv = """
            PlantTypeName,PlantVarietyName,Type,QuantityPurchased,QuantityRemaining,TotalCost,PurchaseDate,SupplierName,SupplierType,Notes
            Pepper,Jalapeño,Plant,6,6,12.00,2025-05-01,,,
            """;

        var existingType = new PlantType { Id = 5, Name = "Pepper" };
        var existingVariety = new PlantVariety { Id = 20, PlantTypeId = 5, Name = "Jalapeño" };

        _plantTypeRepo.Setup(r => r.GetByNameAsync("Pepper")).ReturnsAsync(existingType);
        _varietyRepo.Setup(r => r.GetByPlantTypeAndNameAsync(5, "Jalapeño")).ReturnsAsync(existingVariety);

        var result = await _sut.ImportInventoryAsync(1, MakeCsv(csv));

        result.Errors.Should().BeEmpty();
        _plantTypeRepo.Verify(r => r.CreateAsync(It.IsAny<PlantType>()), Times.Never);
        _varietyRepo.Verify(r => r.CreateAsync(It.IsAny<PlantVariety>()), Times.Never);
    }

    [Fact]
    public async Task ImportInventoryAsync_WithSupplier_FindsOrCreatesSupplier()
    {
        const string csv = """
            PlantTypeName,PlantVarietyName,Type,QuantityPurchased,QuantityRemaining,TotalCost,PurchaseDate,SupplierName,SupplierType,Notes
            Lettuce,Butterhead,Seed,100,100,3.00,2025-02-01,Baker Creek,Online,
            """;

        _plantTypeRepo.Setup(r => r.GetByNameAsync("Lettuce")).ReturnsAsync(new PlantType { Id = 2, Name = "Lettuce" });
        _varietyRepo.Setup(r => r.GetByPlantTypeAndNameAsync(2, "Butterhead")).ReturnsAsync(new PlantVariety { Id = 15, PlantTypeId = 2, Name = "Butterhead" });
        _supplierRepo.Setup(r => r.GetByNameAsync("Baker Creek")).ReturnsAsync((Supplier?)null);
        _supplierRepo.Setup(r => r.CreateAsync(It.IsAny<Supplier>())).ReturnsAsync(7);

        var result = await _sut.ImportInventoryAsync(1, MakeCsv(csv));

        result.Errors.Should().BeEmpty();
        _supplierRepo.Verify(r => r.CreateAsync(It.Is<Supplier>(s =>
            s.Name == "Baker Creek" && s.SupplierType == SupplierType.Online)), Times.Once);
        _inventoryRepo.Verify(r => r.CreateAsync(It.Is<InventoryItem>(i => i.SupplierId == 7)), Times.Once);
    }

    [Fact]
    public async Task ImportInventoryAsync_ExistingSupplier_ReusesIt()
    {
        const string csv = """
            PlantTypeName,PlantVarietyName,Type,QuantityPurchased,QuantityRemaining,TotalCost,PurchaseDate,SupplierName,SupplierType,Notes
            Squash,Butternut,Seed,20,20,2.50,2025-04-01,Local Farm,,
            """;

        _plantTypeRepo.Setup(r => r.GetByNameAsync("Squash")).ReturnsAsync(new PlantType { Id = 3, Name = "Squash" });
        _varietyRepo.Setup(r => r.GetByPlantTypeAndNameAsync(3, "Butternut")).ReturnsAsync(new PlantVariety { Id = 30, PlantTypeId = 3, Name = "Butternut" });
        _supplierRepo.Setup(r => r.GetByNameAsync("Local Farm")).ReturnsAsync(new Supplier { Id = 9, Name = "Local Farm", SupplierType = SupplierType.LocalNursery });

        await _sut.ImportInventoryAsync(1, MakeCsv(csv));

        _supplierRepo.Verify(r => r.CreateAsync(It.IsAny<Supplier>()), Times.Never);
        _inventoryRepo.Verify(r => r.CreateAsync(It.Is<InventoryItem>(i => i.SupplierId == 9)), Times.Once);
    }

    [Fact]
    public async Task ImportInventoryAsync_ExistingInventoryItem_UpdatesInsteadOfCreating()
    {
        const string csv = """
            PlantTypeName,PlantVarietyName,Type,QuantityPurchased,QuantityRemaining,TotalCost,PurchaseDate,SupplierName,SupplierType,Notes
            Tomato,Roma,Seed,100,80,5.00,2025-03-01,,,Updated notes
            """;

        var existingType = new PlantType { Id = 1, Name = "Tomato" };
        var existingVariety = new PlantVariety { Id = 11, PlantTypeId = 1, Name = "Roma" };
        var existingItem = new InventoryItem
        {
            Id = 55, UserId = 1, PlantVarietyId = 11,
            Type = InventoryType.Seed,
            QuantityPurchased = 50, QuantityRemaining = 40,
            TotalCost = 3.00m,
            PurchaseDate = new DateOnly(2025, 3, 1)
        };

        _plantTypeRepo.Setup(r => r.GetByNameAsync("Tomato")).ReturnsAsync(existingType);
        _varietyRepo.Setup(r => r.GetByPlantTypeAndNameAsync(1, "Roma")).ReturnsAsync(existingVariety);
        _inventoryRepo.Setup(r => r.GetByUserAsync(1)).ReturnsAsync([existingItem]);

        var result = await _sut.ImportInventoryAsync(1, MakeCsv(csv));

        result.Updated.Should().Be(1);
        result.Created.Should().Be(0);
        _inventoryRepo.Verify(r => r.CreateAsync(It.IsAny<InventoryItem>()), Times.Never);
        _inventoryRepo.Verify(r => r.UpdateAsync(It.Is<InventoryItem>(i =>
            i.Id == 55 &&
            i.QuantityPurchased == 100 &&
            i.TotalCost == 5.00m &&
            i.Notes == "Updated notes")), Times.Once);
        _inventoryRepo.Verify(r => r.UpdateRemainingQuantityAsync(55, 80), Times.Once);
    }

    [Fact]
    public async Task ImportInventoryAsync_InvalidInventoryType_RecordsErrorAndContinues()
    {
        const string csv = """
            PlantTypeName,PlantVarietyName,Type,QuantityPurchased,QuantityRemaining,TotalCost,PurchaseDate,SupplierName,SupplierType,Notes
            Tomato,Roma,BADTYPE,10,10,2.00,2025-01-01,,,
            Pepper,Jalapeño,Plant,6,6,8.00,2025-01-01,,,
            """;

        _plantTypeRepo.Setup(r => r.GetByNameAsync("Tomato")).ReturnsAsync((PlantType?)null);
        _plantTypeRepo.Setup(r => r.GetByNameAsync("Pepper")).ReturnsAsync(new PlantType { Id = 2, Name = "Pepper" });
        _plantTypeRepo.Setup(r => r.CreateAsync(It.IsAny<PlantType>())).ReturnsAsync(1);
        _varietyRepo.Setup(r => r.GetByPlantTypeAndNameAsync(2, "Jalapeño")).ReturnsAsync(new PlantVariety { Id = 5, PlantTypeId = 2, Name = "Jalapeño" });

        var result = await _sut.ImportInventoryAsync(1, MakeCsv(csv));

        result.Errors.Should().HaveCount(1);
        result.Errors[0].Should().Contain("BADTYPE");
        result.Created.Should().Be(1); // Jalapeño row succeeds
    }

    [Fact]
    public async Task ImportInventoryAsync_InvalidDate_RecordsError()
    {
        const string csv = """
            PlantTypeName,PlantVarietyName,Type,QuantityPurchased,QuantityRemaining,TotalCost,PurchaseDate,SupplierName,SupplierType,Notes
            Tomato,Roma,Seed,10,10,2.00,not-a-date,,,
            """;

        _plantTypeRepo.Setup(r => r.GetByNameAsync("Tomato")).ReturnsAsync(new PlantType { Id = 1, Name = "Tomato" });
        _varietyRepo.Setup(r => r.GetByPlantTypeAndNameAsync(1, "Roma")).ReturnsAsync(new PlantVariety { Id = 5, PlantTypeId = 1, Name = "Roma" });

        var result = await _sut.ImportInventoryAsync(1, MakeCsv(csv));

        result.Errors.Should().HaveCount(1);
        result.Errors[0].Should().Contain("not-a-date");
        result.Created.Should().Be(0);
    }

    [Fact]
    public async Task ImportInventoryAsync_MissingPlantTypeName_RecordsError()
    {
        const string csv = """
            PlantTypeName,PlantVarietyName,Type,QuantityPurchased,QuantityRemaining,TotalCost,PurchaseDate,SupplierName,SupplierType,Notes
            ,Roma,Seed,10,10,2.00,2025-01-01,,,
            """;

        var result = await _sut.ImportInventoryAsync(1, MakeCsv(csv));

        result.Errors.Should().HaveCount(1);
        result.Errors[0].Should().Contain("PlantTypeName");
        result.Created.Should().Be(0);
    }

    [Fact]
    public async Task ImportInventoryAsync_EmptyCsv_ReturnsZeroCounts()
    {
        const string csv = "PlantTypeName,PlantVarietyName,Type,QuantityPurchased,QuantityRemaining,TotalCost,PurchaseDate,SupplierName,SupplierType,Notes\n";

        var result = await _sut.ImportInventoryAsync(1, MakeCsv(csv));

        result.Created.Should().Be(0);
        result.Updated.Should().Be(0);
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ImportInventoryAsync_SameRowTwice_CreateOnlyOnce()
    {
        const string csv = """
            PlantTypeName,PlantVarietyName,Type,QuantityPurchased,QuantityRemaining,TotalCost,PurchaseDate,SupplierName,SupplierType,Notes
            Tomato,Roma,Seed,20,20,3.00,2025-06-01,,,
            Tomato,Roma,Seed,20,20,3.00,2025-06-01,,,
            """;

        _plantTypeRepo.SetupSequence(r => r.GetByNameAsync("Tomato"))
            .ReturnsAsync((PlantType?)null)
            .ReturnsAsync(new PlantType { Id = 1, Name = "Tomato" });
        _plantTypeRepo.Setup(r => r.CreateAsync(It.IsAny<PlantType>())).ReturnsAsync(1);
        _varietyRepo.SetupSequence(r => r.GetByPlantTypeAndNameAsync(1, "Roma"))
            .ReturnsAsync((PlantVariety?)null)
            .ReturnsAsync(new PlantVariety { Id = 5, PlantTypeId = 1, Name = "Roma" });
        _varietyRepo.Setup(r => r.CreateAsync(It.IsAny<PlantVariety>())).ReturnsAsync(5);
        _inventoryRepo.Setup(r => r.CreateAsync(It.IsAny<InventoryItem>())).ReturnsAsync(99);

        var result = await _sut.ImportInventoryAsync(1, MakeCsv(csv));

        result.Created.Should().Be(1);
        result.Updated.Should().Be(1); // second row matches and updates
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ImportInventoryAsync_UnknownSupplierType_DefaultsToOther()
    {
        const string csv = """
            PlantTypeName,PlantVarietyName,Type,QuantityPurchased,QuantityRemaining,TotalCost,PurchaseDate,SupplierName,SupplierType,Notes
            Kale,Red Russian,Seed,50,50,2.00,2025-01-15,Mystery Source,InvalidType,
            """;

        _plantTypeRepo.Setup(r => r.GetByNameAsync("Kale")).ReturnsAsync(new PlantType { Id = 4, Name = "Kale" });
        _varietyRepo.Setup(r => r.GetByPlantTypeAndNameAsync(4, "Red Russian")).ReturnsAsync(new PlantVariety { Id = 40, PlantTypeId = 4, Name = "Red Russian" });
        _supplierRepo.Setup(r => r.GetByNameAsync("Mystery Source")).ReturnsAsync((Supplier?)null);
        _supplierRepo.Setup(r => r.CreateAsync(It.IsAny<Supplier>())).ReturnsAsync(20);

        await _sut.ImportInventoryAsync(1, MakeCsv(csv));

        _supplierRepo.Verify(r => r.CreateAsync(It.Is<Supplier>(s => s.SupplierType == SupplierType.Other)), Times.Once);
    }
}
