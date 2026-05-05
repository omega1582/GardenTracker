using FluentAssertions;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Enums;
using GardenTracker.Core.Interfaces.Repositories;
using GardenTracker.Services;
using Moq;

namespace GardenTracker.Tests.Services;

public class MarketPriceServiceTests
{
    private readonly Mock<IMarketPriceRepository> _priceRepo = new();
    private readonly Mock<ISeasonRepository> _seasonRepo = new();
    private readonly Mock<IGardenRepository> _gardenRepo = new();
    private readonly MarketPriceService _sut;

    public MarketPriceServiceTests() =>
        _sut = new MarketPriceService(_priceRepo.Object, _seasonRepo.Object, _gardenRepo.Object);

    [Fact]
    public async Task GetBySeasonAsync_ReturnsEmpty_WhenUserDoesNotOwnGarden()
    {
        _gardenRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Garden { Id = 1, UserId = 42 });

        var result = await _sut.GetBySeasonAsync(gardenId: 1, year: 2025, userId: 99);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateAsync_AssignsSeasonId()
    {
        var season = new Season { Id = 10, GardenId = 1, Year = 2025 };
        _seasonRepo.Setup(r => r.GetByYearAsync(1, 2025)).ReturnsAsync(season);
        _priceRepo.Setup(r => r.CreateAsync(It.IsAny<MarketPrice>())).ReturnsAsync(7);

        var price = new MarketPrice
        {
            PlantTypeId = 1, PricePerUnit = 3.20m,
            Unit = HarvestUnit.Pounds, RecordedDate = new DateOnly(2025, 6, 1)
        };
        var result = await _sut.CreateAsync(gardenId: 1, year: 2025, userId: 42, price);

        result.SeasonId.Should().Be(10);
        result.Id.Should().Be(7);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenPriceNotFound()
    {
        _priceRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((MarketPrice?)null);

        var result = await _sut.UpdateAsync(99, userId: 42, 4.00m, HarvestUnit.Pounds, DateOnly.FromDateTime(DateTime.Today));

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_UpdatesPriceAndUnit()
    {
        var price = new MarketPrice { Id = 1, PricePerUnit = 3.20m, Unit = HarvestUnit.Pounds };
        _priceRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(price);

        var newDate = new DateOnly(2025, 8, 1);
        var result = await _sut.UpdateAsync(1, userId: 42, 5.50m, HarvestUnit.Pounds, newDate);

        result.Should().BeTrue();
        _priceRepo.Verify(r => r.UpdateAsync(It.Is<MarketPrice>(m =>
            m.PricePerUnit == 5.50m && m.RecordedDate == newDate)), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenPriceNotFound()
    {
        _priceRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((MarketPrice?)null);

        var result = await _sut.DeleteAsync(99, userId: 42);

        result.Should().BeFalse();
        _priceRepo.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
    }
}
