using FluentAssertions;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Interfaces.Repositories;
using GardenTracker.Services;
using Moq;

namespace GardenTracker.Tests.Services;

public class RaisedBedServiceTests
{
    private readonly Mock<IRaisedBedRepository> _bedRepo = new();
    private readonly Mock<IGardenRepository> _gardenRepo = new();
    private readonly RaisedBedService _sut;

    public RaisedBedServiceTests() => _sut = new RaisedBedService(_bedRepo.Object, _gardenRepo.Object);

    private Garden OwnerGarden => new() { Id = 1, UserId = 42 };

    [Fact]
    public async Task GetByGardenAsync_ReturnsBeds_WhenUserOwnsGarden()
    {
        _gardenRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(OwnerGarden);
        _bedRepo.Setup(r => r.GetByGardenAsync(1)).ReturnsAsync([new RaisedBed { Id = 1 }]);

        var result = await _sut.GetByGardenAsync(gardenId: 1, userId: 42);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByGardenAsync_ReturnsEmpty_WhenUserDoesNotOwnGarden()
    {
        _gardenRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(OwnerGarden);

        var result = await _sut.GetByGardenAsync(gardenId: 1, userId: 99);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenUserDoesNotOwnGarden()
    {
        var bed = new RaisedBed { Id = 5, GardenId = 1 };
        _bedRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(bed);
        _gardenRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(OwnerGarden);

        var result = await _sut.GetByIdAsync(id: 5, userId: 99);

        result.Should().BeNull();
    }

    [Fact]
    public async Task RetireAsync_ReturnsTrue_WhenUserOwnsbed()
    {
        var bed = new RaisedBed { Id = 5, GardenId = 1 };
        _bedRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(bed);
        _gardenRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(OwnerGarden);

        var retireDate = new DateOnly(2025, 10, 1);
        var result = await _sut.RetireAsync(id: 5, userId: 42, retireDate);

        result.Should().BeTrue();
        _bedRepo.Verify(r => r.RetireAsync(5, retireDate), Times.Once);
    }

    [Fact]
    public async Task RetireAsync_ReturnsFalse_WhenUserDoesNotOwnBed()
    {
        var bed = new RaisedBed { Id = 5, GardenId = 1 };
        _bedRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(bed);
        _gardenRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(OwnerGarden);

        var result = await _sut.RetireAsync(id: 5, userId: 99, new DateOnly(2025, 10, 1));

        result.Should().BeFalse();
        _bedRepo.Verify(r => r.RetireAsync(It.IsAny<int>(), It.IsAny<DateOnly>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_SetsGardenId()
    {
        _bedRepo.Setup(r => r.CreateAsync(It.IsAny<RaisedBed>())).ReturnsAsync(7);
        var bed = new RaisedBed { Name = "Bed A", InstalledDate = new DateOnly(2025, 4, 1) };

        var result = await _sut.CreateAsync(gardenId: 1, userId: 42, bed);

        result.GardenId.Should().Be(1);
        result.Id.Should().Be(7);
    }
}
