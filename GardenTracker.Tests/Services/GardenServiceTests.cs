using FluentAssertions;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Interfaces.Repositories;
using GardenTracker.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace GardenTracker.Tests.Services;

public class GardenServiceTests
{
    private readonly Mock<IGardenRepository> _gardenRepo = new();
    private readonly GardenService _sut;

    public GardenServiceTests() => _sut = new GardenService(_gardenRepo.Object, NullLogger<GardenService>.Instance);

    [Fact]
    public async Task GetByIdAsync_ReturnsGarden_WhenUserOwnsIt()
    {
        var garden = new Garden { Id = 1, UserId = 42, Name = "My Garden" };
        _gardenRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(garden);

        var result = await _sut.GetByIdAsync(1, userId: 42);

        result.Should().NotBeNull();
        result!.Name.Should().Be("My Garden");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenUserDoesNotOwnGarden()
    {
        var garden = new Garden { Id = 1, UserId = 42, Name = "My Garden" };
        _gardenRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(garden);

        var result = await _sut.GetByIdAsync(1, userId: 99);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenGardenDoesNotExist()
    {
        _gardenRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Garden?)null);

        var result = await _sut.GetByIdAsync(1, userId: 42);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_SetsUserIdAndCreatedAt()
    {
        _gardenRepo.Setup(r => r.CreateAsync(It.IsAny<Garden>())).ReturnsAsync(5);

        var result = await _sut.CreateAsync(userId: 42, "Test Garden", "Backyard", null);

        result.UserId.Should().Be(42);
        result.Name.Should().Be("Test Garden");
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdateAsync_ReturnsTrue_WhenUserOwnsGarden()
    {
        var garden = new Garden { Id = 1, UserId = 42, Name = "Old Name" };
        _gardenRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(garden);

        var result = await _sut.UpdateAsync(1, userId: 42, "New Name", null, null);

        result.Should().BeTrue();
        _gardenRepo.Verify(r => r.UpdateAsync(It.Is<Garden>(g => g.Name == "New Name")), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenGardenNotFound()
    {
        _gardenRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Garden?)null);

        var result = await _sut.UpdateAsync(99, userId: 42, "Name", null, null);

        result.Should().BeFalse();
        _gardenRepo.Verify(r => r.UpdateAsync(It.IsAny<Garden>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenUserDoesNotOwnGarden()
    {
        var garden = new Garden { Id = 1, UserId = 42, Name = "Old Name" };
        _gardenRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(garden);

        var result = await _sut.UpdateAsync(1, userId: 99, "New Name", null, null);

        result.Should().BeFalse();
        _gardenRepo.Verify(r => r.UpdateAsync(It.IsAny<Garden>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsTrue_WhenUserOwnsGarden()
    {
        var garden = new Garden { Id = 1, UserId = 42 };
        _gardenRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(garden);

        var result = await _sut.DeleteAsync(1, userId: 42);

        result.Should().BeTrue();
        _gardenRepo.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenGardenNotFound()
    {
        _gardenRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Garden?)null);

        var result = await _sut.DeleteAsync(99, userId: 42);

        result.Should().BeFalse();
        _gardenRepo.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenUserDoesNotOwnGarden()
    {
        var garden = new Garden { Id = 1, UserId = 42 };
        _gardenRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(garden);

        var result = await _sut.DeleteAsync(1, userId: 99);

        result.Should().BeFalse();
        _gardenRepo.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
    }
}
