using FluentAssertions;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Interfaces.Repositories;
using GardenTracker.Services;
using Moq;

namespace GardenTracker.Tests.Services;

public class SeasonServiceTests
{
    private readonly Mock<ISeasonRepository> _seasonRepo = new();
    private readonly Mock<IGardenRepository> _gardenRepo = new();
    private readonly SeasonService _sut;

    public SeasonServiceTests() => _sut = new SeasonService(_seasonRepo.Object, _gardenRepo.Object);

    [Fact]
    public async Task GetByGardenAsync_ReturnsSeasons_WhenUserOwnsGarden()
    {
        var garden = new Garden { Id = 1, UserId = 42 };
        var seasons = new List<Season> { new() { Id = 1, GardenId = 1, Year = 2025 } };
        _gardenRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(garden);
        _seasonRepo.Setup(r => r.GetByGardenAsync(1)).ReturnsAsync(seasons);

        var result = await _sut.GetByGardenAsync(gardenId: 1, userId: 42);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByGardenAsync_ReturnsEmpty_WhenUserDoesNotOwnGarden()
    {
        var garden = new Garden { Id = 1, UserId = 42 };
        _gardenRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(garden);

        var result = await _sut.GetByGardenAsync(gardenId: 1, userId: 99);

        result.Should().BeEmpty();
        _seasonRepo.Verify(r => r.GetByGardenAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetByYearAsync_ReturnsNull_WhenUserDoesNotOwnGarden()
    {
        var garden = new Garden { Id = 1, UserId = 42 };
        _gardenRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(garden);

        var result = await _sut.GetByYearAsync(gardenId: 1, year: 2025, userId: 99);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_SetsgGardenIdAndYear()
    {
        _seasonRepo.Setup(r => r.CreateAsync(It.IsAny<Season>())).ReturnsAsync(10);

        var result = await _sut.CreateAsync(gardenId: 1, userId: 42, year: 2025, notes: null);

        result.GardenId.Should().Be(1);
        result.Year.Should().Be(2025);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenSeasonNotFound()
    {
        var garden = new Garden { Id = 1, UserId = 42 };
        _gardenRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(garden);
        _seasonRepo.Setup(r => r.GetByYearAsync(1, 2025)).ReturnsAsync((Season?)null);

        var result = await _sut.UpdateAsync(gardenId: 1, year: 2025, userId: 42, notes: "updated");

        result.Should().BeFalse();
    }
}
