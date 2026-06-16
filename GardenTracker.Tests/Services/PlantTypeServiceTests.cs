using FluentAssertions;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Enums;
using GardenTracker.Core.Interfaces.Repositories;
using GardenTracker.Services;
using Moq;

namespace GardenTracker.Tests.Services;

public class PlantTypeServiceTests
{
    private readonly Mock<IPlantTypeRepository> _repo = new();
    private readonly PlantTypeService _sut;

    public PlantTypeServiceTests() => _sut = new PlantTypeService(_repo.Object);

    [Fact]
    public async Task GetAllAsync_ReturnsAllPlantTypes()
    {
        _repo.Setup(r => r.GetAllAsync()).ReturnsAsync(
        [
            new PlantType { Id = 1, Name = "Tomato" },
            new PlantType { Id = 2, Name = "Pepper" }
        ]);

        var result = await _sut.GetAllAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateAsync_SetsAllFields()
    {
        _repo.Setup(r => r.CreateAsync(It.IsAny<PlantType>())).ReturnsAsync(3);

        var result = await _sut.CreateAsync("Lettuce", GrowthHabit.Rosette, 45, 6, SunPreference.PartialSun, false);

        result.Name.Should().Be("Lettuce");
        result.Id.Should().Be(3);
        result.GrowthHabit.Should().Be(GrowthHabit.Rosette);
        result.DaysToMaturity.Should().Be(45);
        result.SpacingInches.Should().Be(6);
        result.SunPreference.Should().Be(SunPreference.PartialSun);
        result.IsPerennial.Should().BeFalse();
    }

    [Fact]
    public async Task CreateAsync_AllowsNullExtendedFields()
    {
        _repo.Setup(r => r.CreateAsync(It.IsAny<PlantType>())).ReturnsAsync(4);

        var result = await _sut.CreateAsync("Mint", null, null, null, null, null);

        result.GrowthHabit.Should().BeNull();
        result.DaysToMaturity.Should().BeNull();
        result.SpacingInches.Should().BeNull();
        result.SunPreference.Should().BeNull();
        result.IsPerennial.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ReturnsTrue_WhenPlantTypeExists()
    {
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new PlantType { Id = 1, Name = "Old" });

        var result = await _sut.UpdateAsync(1, "New", GrowthHabit.Upright, 70, 18, SunPreference.FullSun, false);

        result.Should().BeTrue();
        _repo.Verify(r => r.UpdateAsync(It.Is<PlantType>(p =>
            p.Name == "New" &&
            p.GrowthHabit == GrowthHabit.Upright &&
            p.DaysToMaturity == 70 &&
            p.SpacingInches == 18 &&
            p.SunPreference == SunPreference.FullSun &&
            p.IsPerennial == false)), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenPlantTypeNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((PlantType?)null);

        var result = await _sut.UpdateAsync(99, "New", null, null, null, null, null);

        result.Should().BeFalse();
        _repo.Verify(r => r.UpdateAsync(It.IsAny<PlantType>()), Times.Never);
    }
}
