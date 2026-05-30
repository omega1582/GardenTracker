using FluentAssertions;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Enums;
using GardenTracker.Core.Interfaces.Repositories;
using GardenTracker.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace GardenTracker.Tests.Services;

public class PlantingServiceTests
{
    private readonly Mock<IPlantingRepository> _plantingRepo = new();
    private readonly Mock<ISeasonRepository> _seasonRepo = new();
    private readonly Mock<IGardenRepository> _gardenRepo = new();
    private readonly Mock<IInventoryRepository> _inventoryRepo = new();
    private readonly PlantingService _sut;

    public PlantingServiceTests() =>
        _sut = new PlantingService(_plantingRepo.Object, _seasonRepo.Object, _gardenRepo.Object, _inventoryRepo.Object, NullLogger<PlantingService>.Instance);

    private void SetupOwnership(BedPlanting planting, int userId)
    {
        var season = new Season { Id = planting.SeasonId, GardenId = 1 };
        _seasonRepo.Setup(r => r.GetByIdAsync(planting.SeasonId)).ReturnsAsync(season);
        _gardenRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Garden { Id = 1, UserId = userId });
    }

    [Fact]
    public async Task GetBySeasonAsync_ReturnsEmpty_WhenUserDoesNotOwnGarden()
    {
        _gardenRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Garden { Id = 1, UserId = 42 });

        var result = await _sut.GetBySeasonAsync(gardenId: 1, year: 2025, userId: 99, null, null);

        result.Should().BeEmpty();
        _plantingRepo.Verify(r => r.GetBySeasonAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsPlanting_WhenUserOwnsIt()
    {
        var planting = new BedPlanting { Id = 1, SeasonId = 10 };
        _plantingRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(planting);
        SetupOwnership(planting, userId: 42);

        var result = await _sut.GetByIdAsync(1, userId: 42);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenUserDoesNotOwnPlanting()
    {
        var planting = new BedPlanting { Id = 1, SeasonId = 10 };
        _plantingRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(planting);
        SetupOwnership(planting, userId: 42);

        var result = await _sut.GetByIdAsync(1, userId: 99);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenPlantingNotFound()
    {
        _plantingRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((BedPlanting?)null);

        var result = await _sut.GetByIdAsync(99, userId: 42);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ThrowsInvalidOperationException_WhenSeasonDoesNotExist()
    {
        _seasonRepo.Setup(r => r.GetByYearAsync(1, 2025)).ReturnsAsync((Season?)null);

        var act = async () => await _sut.CreateAsync(gardenId: 1, year: 2025, userId: 42,
            new BedPlanting { BedId = 2, PlantVarietyId = 3 }, quantityUsedFromInventory: null);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No season found*");
    }

    [Fact]
    public async Task CreateAsync_AssignsSeasonId()
    {
        var season = new Season { Id = 10, GardenId = 1, Year = 2025 };
        _seasonRepo.Setup(r => r.GetByYearAsync(1, 2025)).ReturnsAsync(season);
        _plantingRepo.Setup(r => r.CreateAsync(It.IsAny<BedPlanting>())).ReturnsAsync(99);

        var planting = new BedPlanting { BedId = 2, PlantVarietyId = 3, TotalCost = 4.50m };
        var result = await _sut.CreateAsync(gardenId: 1, year: 2025, userId: 42, planting, quantityUsedFromInventory: null);

        result.SeasonId.Should().Be(10);
        result.Id.Should().Be(99);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenPlantingNotFound()
    {
        _plantingRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((BedPlanting?)null);

        var result = await _sut.UpdateAsync(99, userId: 42, null, StartMethod.Seed, 2, 5.00m, null, null, null, null);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenUserDoesNotOwnPlanting()
    {
        var planting = new BedPlanting { Id = 1, SeasonId = 10 };
        _plantingRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(planting);
        SetupOwnership(planting, userId: 42);

        var result = await _sut.UpdateAsync(1, userId: 99, null, StartMethod.Seed, 2, 5.00m, null, null, null, null);

        result.Should().BeFalse();
        _plantingRepo.Verify(r => r.UpdateAsync(It.IsAny<BedPlanting>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesFields()
    {
        var planting = new BedPlanting { Id = 1, SeasonId = 10, StartMethod = StartMethod.Seed, Quantity = 1, TotalCost = 2.00m };
        _plantingRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(planting);
        SetupOwnership(planting, userId: 42);

        var result = await _sut.UpdateAsync(1, userId: 42, supplierId: 5, StartMethod.Transplant, quantity: 3, totalCost: 12.00m, null, "updated", null, null);

        result.Should().BeTrue();
        _plantingRepo.Verify(r => r.UpdateAsync(It.Is<BedPlanting>(p =>
            p.SupplierId == 5 &&
            p.StartMethod == StartMethod.Transplant &&
            p.Quantity == 3 &&
            p.TotalCost == 12.00m &&
            p.Notes == "updated")), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenPlantingNotFound()
    {
        _plantingRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((BedPlanting?)null);

        var result = await _sut.DeleteAsync(99, userId: 42);

        result.Should().BeFalse();
        _plantingRepo.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenUserDoesNotOwnPlanting()
    {
        var planting = new BedPlanting { Id = 1, SeasonId = 10 };
        _plantingRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(planting);
        SetupOwnership(planting, userId: 42);

        var result = await _sut.DeleteAsync(1, userId: 99);

        result.Should().BeFalse();
        _plantingRepo.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    // ── Inventory deduction ─────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_DeductsFromInventory_WhenInventoryLinked()
    {
        var season = new Season { Id = 10, GardenId = 1, Year = 2025 };
        _seasonRepo.Setup(r => r.GetByYearAsync(1, 2025)).ReturnsAsync(season);
        _plantingRepo.Setup(r => r.CreateAsync(It.IsAny<BedPlanting>())).ReturnsAsync(1);

        var inventoryItem = new InventoryItem { Id = 5, QuantityRemaining = 50 };
        _inventoryRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(inventoryItem);

        var planting = new BedPlanting { BedId = 2, PlantVarietyId = 3, InventoryItemId = 5 };
        await _sut.CreateAsync(gardenId: 1, year: 2025, userId: 42, planting, quantityUsedFromInventory: 12);

        _inventoryRepo.Verify(r => r.UpdateRemainingQuantityAsync(5, 38), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_DoesNotTouchInventory_WhenNoInventoryLink()
    {
        var season = new Season { Id = 10, GardenId = 1, Year = 2025 };
        _seasonRepo.Setup(r => r.GetByYearAsync(1, 2025)).ReturnsAsync(season);
        _plantingRepo.Setup(r => r.CreateAsync(It.IsAny<BedPlanting>())).ReturnsAsync(1);

        var planting = new BedPlanting { BedId = 2, PlantVarietyId = 3 };
        await _sut.CreateAsync(gardenId: 1, year: 2025, userId: 42, planting, quantityUsedFromInventory: null);

        _inventoryRepo.Verify(r => r.UpdateRemainingQuantityAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_RestoresInventory_WhenInventoryWasLinked()
    {
        var planting = new BedPlanting { Id = 1, SeasonId = 10, InventoryItemId = 5, QuantityUsedFromInventory = 12 };
        _plantingRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(planting);
        SetupOwnership(planting, userId: 42);

        var inventoryItem = new InventoryItem { Id = 5, QuantityRemaining = 20 };
        _inventoryRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(inventoryItem);

        await _sut.DeleteAsync(1, userId: 42);

        // 20 remaining + 12 restored = 32
        _inventoryRepo.Verify(r => r.UpdateRemainingQuantityAsync(5, 32), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_RestoresOldInventoryAndDeductsNew_WhenInventoryChanges()
    {
        // Planting was linked to item 5, used 10. Now relinking to item 6, using 8.
        var planting = new BedPlanting { Id = 1, SeasonId = 10, InventoryItemId = 5, QuantityUsedFromInventory = 10 };
        _plantingRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(planting);
        SetupOwnership(planting, userId: 42);

        var oldItem = new InventoryItem { Id = 5, QuantityRemaining = 30 };
        var newItem = new InventoryItem { Id = 6, QuantityRemaining = 50 };
        _inventoryRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(oldItem);
        _inventoryRepo.Setup(r => r.GetByIdAsync(6)).ReturnsAsync(newItem);

        await _sut.UpdateAsync(1, userId: 42, null, StartMethod.Seed, 6, 0m, null, null,
            inventoryItemId: 6, quantityUsedFromInventory: 8);

        // Old item: 30 + 10 restored = 40
        _inventoryRepo.Verify(r => r.UpdateRemainingQuantityAsync(5, 40), Times.Once);
        // New item: 50 - 8 deducted = 42
        _inventoryRepo.Verify(r => r.UpdateRemainingQuantityAsync(6, 42), Times.Once);
    }
}
