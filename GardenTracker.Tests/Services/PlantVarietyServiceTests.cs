using FluentAssertions;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Interfaces.Repositories;
using GardenTracker.Services;
using Moq;

namespace GardenTracker.Tests.Services;

public class PlantVarietyServiceTests
{
    private readonly Mock<IPlantVarietyRepository> _repo = new();
    private readonly PlantVarietyService _sut;

    public PlantVarietyServiceTests() => _sut = new PlantVarietyService(_repo.Object);

    [Fact]
    public async Task GetByPlantTypeAsync_ReturnsVarietiesForType()
    {
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
    public async Task CreateAsync_SetsPlantTypeIdAndName()
    {
        _repo.Setup(r => r.CreateAsync(It.IsAny<PlantVariety>())).ReturnsAsync(5);

        var result = await _sut.CreateAsync(plantTypeId: 1, "Brandywine", notes: "Heirloom");

        result.PlantTypeId.Should().Be(1);
        result.Name.Should().Be("Brandywine");
        result.Notes.Should().Be("Heirloom");
        result.Id.Should().Be(5);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenVarietyNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((PlantVariety?)null);

        var result = await _sut.UpdateAsync(99, "New Name", null);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_UpdatesNameAndNotes()
    {
        var variety = new PlantVariety { Id = 1, PlantTypeId = 1, Name = "Old", Notes = "Old notes" };
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(variety);

        var result = await _sut.UpdateAsync(1, "New Name", "New notes");

        result.Should().BeTrue();
        _repo.Verify(r => r.UpdateAsync(It.Is<PlantVariety>(v =>
            v.Name == "New Name" && v.Notes == "New notes")), Times.Once);
    }
}
