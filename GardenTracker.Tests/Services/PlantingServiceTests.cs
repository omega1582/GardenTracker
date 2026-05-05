using FluentAssertions;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Enums;
using GardenTracker.Core.Interfaces.Repositories;
using GardenTracker.Services;
using Moq;

namespace GardenTracker.Tests.Services;

public class PlantingServiceTests
{
    private readonly Mock<IPlantingRepository> _plantingRepo = new();
    private readonly Mock<ISeasonRepository> _seasonRepo = new();
    private readonly Mock<IGardenRepository> _gardenRepo = new();
    private readonly PlantingService _sut;

    public PlantingServiceTests() =>
        _sut = new PlantingService(_plantingRepo.Object, _seasonRepo.Object, _gardenRepo.Object);

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
    public async Task CreateAsync_AssignsSeasonId()
    {
        var season = new Season { Id = 10, GardenId = 1, Year = 2025 };
        _seasonRepo.Setup(r => r.GetByYearAsync(1, 2025)).ReturnsAsync(season);
        _plantingRepo.Setup(r => r.CreateAsync(It.IsAny<BedPlanting>())).ReturnsAsync(99);

        var planting = new BedPlanting { BedId = 2, PlantVarietyId = 3, TotalCost = 4.50m };
        var result = await _sut.CreateAsync(gardenId: 1, year: 2025, userId: 42, planting);

        result.SeasonId.Should().Be(10);
        result.Id.Should().Be(99);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenPlantingNotFound()
    {
        _plantingRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((BedPlanting?)null);

        var result = await _sut.UpdateAsync(99, userId: 42, null, StartMethod.Seed, 2, 5.00m, null, null);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenUserDoesNotOwnPlanting()
    {
        var planting = new BedPlanting { Id = 1, SeasonId = 10 };
        _plantingRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(planting);
        SetupOwnership(planting, userId: 42);

        var result = await _sut.UpdateAsync(1, userId: 99, null, StartMethod.Seed, 2, 5.00m, null, null);

        result.Should().BeFalse();
        _plantingRepo.Verify(r => r.UpdateAsync(It.IsAny<BedPlanting>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesFields()
    {
        var planting = new BedPlanting { Id = 1, SeasonId = 10, StartMethod = StartMethod.Seed, Quantity = 1, TotalCost = 2.00m };
        _plantingRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(planting);
        SetupOwnership(planting, userId: 42);

        var result = await _sut.UpdateAsync(1, userId: 42, supplierId: 5, StartMethod.Transplant, quantity: 3, totalCost: 12.00m, null, "updated");

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
}
