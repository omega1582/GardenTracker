using FluentAssertions;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Enums;
using GardenTracker.Core.Interfaces.Repositories;
using GardenTracker.Services;
using Moq;

namespace GardenTracker.Tests.Services;

public class PlantVarietyServiceTests
{
    private readonly Mock<IPlantVarietyRepository> _repo = new();
    private readonly Mock<IPlantTypeRepository> _typeRepo = new();
    private readonly PlantVarietyService _sut;

    public PlantVarietyServiceTests() => _sut = new PlantVarietyService(_repo.Object, _typeRepo.Object);

    [Fact]
    public async Task GetByPlantTypeAsync_ReturnsVarietiesForType()
    {
        _typeRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new PlantType { Id = 1, Name = "Tomato" });
        _repo.Setup(r => r.GetByPlantTypeAsync(1)).ReturnsAsync(
        [
            new PlantVariety { Id = 1, PlantTypeId = 1, Name = "Cherokee Purple" },
            new PlantVariety { Id = 2, PlantTypeId = 1, Name = "Roma" }
        ]);

        var result = await _sut.GetByPlantTypeAsync(plantTypeId: 1);

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(v => v.PlantTypeId.Should().Be(1));
    }

    [Fact]
    public async Task CreateAsync_SetsAllFields()
    {
        _repo.Setup(r => r.CreateAsync(It.IsAny<PlantVariety>())).ReturnsAsync(5);

        var result = await _sut.CreateAsync(1, "Brandywine", "Heirloom", GrowthHabit.Upright, 80, 24, SunPreference.FullSun, false, "http://example.com/brandywine.jpg");

        result.PlantTypeId.Should().Be(1);
        result.Name.Should().Be("Brandywine");
        result.Notes.Should().Be("Heirloom");
        result.Id.Should().Be(5);
        result.GrowthHabit.Should().Be(GrowthHabit.Upright);
        result.DaysToMaturity.Should().Be(80);
        result.SpacingInches.Should().Be(24);
        result.SunPreference.Should().Be(SunPreference.FullSun);
        result.IsPerennial.Should().BeFalse();
        result.ImageUrl.Should().Be("http://example.com/brandywine.jpg");
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenVarietyNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((PlantVariety?)null);

        var result = await _sut.UpdateAsync(99, "New Name", null, null, null, null, null, null, null);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_UpdatesAllFields()
    {
        var variety = new PlantVariety { Id = 1, PlantTypeId = 1, Name = "Old", Notes = "Old notes", ImageUrl = "Old image" };
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(variety);

        var result = await _sut.UpdateAsync(1, "New Name", "New notes", GrowthHabit.Vining, 90, 36, SunPreference.FullSun, false, "http://example.com/new.jpg");

        result.Should().BeTrue();
        _repo.Verify(r => r.UpdateAsync(It.Is<PlantVariety>(v =>
            v.Name == "New Name" &&
            v.Notes == "New notes" &&
            v.GrowthHabit == GrowthHabit.Vining &&
            v.DaysToMaturity == 90 &&
            v.SpacingInches == 36 &&
            v.SunPreference == SunPreference.FullSun &&
            v.IsPerennial == false &&
            v.ImageUrl == "http://example.com/new.jpg")), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_AppliesFallback_WhenVarietyFieldsAreNull()
    {
        var plantType = new PlantType
        {
            Id = 1, Name = "Tomato",
            GrowthHabit = GrowthHabit.Upright,
            DaysToMaturity = 75,
            SpacingInches = 24,
            SunPreference = SunPreference.FullSun,
            IsPerennial = false
        };
        var variety = new PlantVariety { Id = 1, PlantTypeId = 1, Name = "Cherokee Purple" };

        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(variety);
        _typeRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(plantType);

        var result = await _sut.GetByIdAsync(1);

        result!.GrowthHabit.Should().Be(GrowthHabit.Upright);
        result.DaysToMaturity.Should().Be(75);
        result.SpacingInches.Should().Be(24);
        result.SunPreference.Should().Be(SunPreference.FullSun);
        result.IsPerennial.Should().BeFalse();
    }

    [Fact]
    public async Task GetByIdAsync_VarietyOwnValueWins_OverPlantTypeFallback()
    {
        var plantType = new PlantType
        {
            Id = 1, Name = "Squash",
            GrowthHabit = GrowthHabit.Vining,
            DaysToMaturity = 50,
            SpacingInches = 36,
            SunPreference = SunPreference.FullSun,
            IsPerennial = false
        };
        var variety = new PlantVariety
        {
            Id = 2, PlantTypeId = 1, Name = "Butternut",
            GrowthHabit = GrowthHabit.Spreading,
            DaysToMaturity = 110,
            SpacingInches = 48,
            SunPreference = SunPreference.FullSun,
            IsPerennial = false
        };

        _repo.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(variety);
        _typeRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(plantType);

        var result = await _sut.GetByIdAsync(2);

        result!.GrowthHabit.Should().Be(GrowthHabit.Spreading);
        result.DaysToMaturity.Should().Be(110);
        result.SpacingInches.Should().Be(48);
    }

    [Fact]
    public async Task GetByIdAsync_PartialFallback_SomeVarietyFieldsSomeFromType()
    {
        var plantType = new PlantType
        {
            Id = 1, Name = "Pepper",
            GrowthHabit = GrowthHabit.Upright,
            DaysToMaturity = 70,
            SunPreference = SunPreference.FullSun
        };
        var variety = new PlantVariety
        {
            Id = 3, PlantTypeId = 1, Name = "Jalapeño",
            DaysToMaturity = 80
        };

        _repo.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(variety);
        _typeRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(plantType);

        var result = await _sut.GetByIdAsync(3);

        result!.GrowthHabit.Should().Be(GrowthHabit.Upright);
        result.DaysToMaturity.Should().Be(80);
        result.SunPreference.Should().Be(SunPreference.FullSun);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenVarietyNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((PlantVariety?)null);

        var result = await _sut.GetByIdAsync(99);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByPlantTypeAsync_AppliesFallbackToAllVarieties()
    {
        var plantType = new PlantType
        {
            Id = 1, Name = "Tomato",
            GrowthHabit = GrowthHabit.Upright,
            SunPreference = SunPreference.FullSun,
            IsPerennial = false
        };
        var varieties = new List<PlantVariety>
        {
            new() { Id = 1, PlantTypeId = 1, Name = "Roma" },
            new() { Id = 2, PlantTypeId = 1, Name = "Cherry", GrowthHabit = GrowthHabit.Bushy }
        };

        _typeRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(plantType);
        _repo.Setup(r => r.GetByPlantTypeAsync(1)).ReturnsAsync(varieties);

        var result = (await _sut.GetByPlantTypeAsync(1)).ToList();

        result[0].GrowthHabit.Should().Be(GrowthHabit.Upright);
        result[1].GrowthHabit.Should().Be(GrowthHabit.Bushy);
        result.Should().AllSatisfy(v => v.SunPreference.Should().Be(SunPreference.FullSun));
        result.Should().AllSatisfy(v => v.IsPerennial.Should().BeFalse());
    }
}
