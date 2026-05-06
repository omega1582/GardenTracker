using FluentAssertions;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Enums;
using GardenTracker.Core.Interfaces.Repositories;
using GardenTracker.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace GardenTracker.Tests.Services;

public class HarvestServiceTests
{
    private readonly Mock<IHarvestRepository> _harvestRepo = new();
    private readonly Mock<ISeasonRepository> _seasonRepo = new();
    private readonly Mock<IGardenRepository> _gardenRepo = new();
    private readonly HarvestService _sut;

    public HarvestServiceTests() =>
        _sut = new HarvestService(_harvestRepo.Object, _seasonRepo.Object, _gardenRepo.Object, NullLogger<HarvestService>.Instance);

    private void SetupOwnership(Harvest harvest, int userId)
    {
        var season = new Season { Id = harvest.SeasonId, GardenId = 1 };
        _seasonRepo.Setup(r => r.GetByIdAsync(harvest.SeasonId)).ReturnsAsync(season);
        _gardenRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Garden { Id = 1, UserId = userId });
    }

    [Fact]
    public async Task GetBySeasonAsync_ReturnsEmpty_WhenUserDoesNotOwnGarden()
    {
        _gardenRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Garden { Id = 1, UserId = 42 });

        var result = await _sut.GetBySeasonAsync(gardenId: 1, year: 2025, userId: 99, null, null);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsHarvest_WhenUserOwnsIt()
    {
        var harvest = new Harvest { Id = 1, SeasonId = 10 };
        _harvestRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(harvest);
        SetupOwnership(harvest, userId: 42);

        var result = await _sut.GetByIdAsync(1, userId: 42);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenUserDoesNotOwnHarvest()
    {
        var harvest = new Harvest { Id = 1, SeasonId = 10 };
        _harvestRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(harvest);
        SetupOwnership(harvest, userId: 42);

        var result = await _sut.GetByIdAsync(1, userId: 99);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_AssignsSeasonId()
    {
        var season = new Season { Id = 10, GardenId = 1, Year = 2025 };
        _seasonRepo.Setup(r => r.GetByYearAsync(1, 2025)).ReturnsAsync(season);
        _harvestRepo.Setup(r => r.CreateAsync(It.IsAny<Harvest>())).ReturnsAsync(55);

        var harvest = new Harvest { BedId = 2, PlantVarietyId = 3, Quantity = 5.5m, Unit = HarvestUnit.Pounds };
        var result = await _sut.CreateAsync(gardenId: 1, year: 2025, userId: 42, harvest);

        result.SeasonId.Should().Be(10);
        result.Id.Should().Be(55);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenHarvestNotFound()
    {
        _harvestRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Harvest?)null);

        var result = await _sut.UpdateAsync(99, userId: 42, 1m, HarvestUnit.Pounds, DateOnly.FromDateTime(DateTime.Today), null);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenUserDoesNotOwnHarvest()
    {
        var harvest = new Harvest { Id = 1, SeasonId = 10 };
        _harvestRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(harvest);
        SetupOwnership(harvest, userId: 42);

        var result = await _sut.UpdateAsync(1, userId: 99, 5m, HarvestUnit.Pounds, DateOnly.FromDateTime(DateTime.Today), null);

        result.Should().BeFalse();
        _harvestRepo.Verify(r => r.UpdateAsync(It.IsAny<Harvest>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesQuantityAndDate()
    {
        var harvest = new Harvest { Id = 1, SeasonId = 10, Quantity = 2m, Unit = HarvestUnit.Pounds };
        _harvestRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(harvest);
        SetupOwnership(harvest, userId: 42);

        var newDate = new DateOnly(2025, 7, 15);
        var result = await _sut.UpdateAsync(1, userId: 42, 10.5m, HarvestUnit.Pounds, newDate, "big harvest");

        result.Should().BeTrue();
        _harvestRepo.Verify(r => r.UpdateAsync(It.Is<Harvest>(h =>
            h.Quantity == 10.5m &&
            h.HarvestDate == newDate &&
            h.Notes == "big harvest")), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenHarvestNotFound()
    {
        _harvestRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Harvest?)null);

        var result = await _sut.DeleteAsync(99, userId: 42);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenUserDoesNotOwnHarvest()
    {
        var harvest = new Harvest { Id = 1, SeasonId = 10 };
        _harvestRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(harvest);
        SetupOwnership(harvest, userId: 42);

        var result = await _sut.DeleteAsync(1, userId: 99);

        result.Should().BeFalse();
        _harvestRepo.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
    }
}
