using System.Text;
using FluentAssertions;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Enums;
using GardenTracker.Core.Interfaces.Repositories;
using GardenTracker.Services;
using Moq;

namespace GardenTracker.Tests.Services;

public class PlantCatalogCsvImportServiceTests
{
    private readonly Mock<IPlantTypeRepository> _typeRepo = new();
    private readonly Mock<IPlantVarietyRepository> _varietyRepo = new();
    private readonly PlantCatalogCsvImportService _sut;

    public PlantCatalogCsvImportServiceTests()
    {
        _sut = new PlantCatalogCsvImportService(_typeRepo.Object, _varietyRepo.Object);

        _typeRepo.Setup(r => r.GetByNameAsync(It.IsAny<string>())).ReturnsAsync((PlantType?)null);
        _typeRepo.Setup(r => r.CreateAsync(It.IsAny<PlantType>())).ReturnsAsync(1);
        _varietyRepo.Setup(r => r.GetByPlantTypeAndNameAsync(It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync((PlantVariety?)null);
        _varietyRepo.Setup(r => r.CreateAsync(It.IsAny<PlantVariety>())).ReturnsAsync(10);
    }

    private static Stream MakeCsv(string content) =>
        new MemoryStream(Encoding.UTF8.GetBytes(content));

    [Fact]
    public async Task ImportAsync_NewTypeOnly_CreatesType()
    {
        const string csv = """
            PlantTypeName,PlantVarietyName,GrowthHabit,DaysToMaturity,SpacingInches,SunPreference,IsPerennial,Notes
            Tomato,,Vining,80,24,FullSun,false,
            """;

        var result = await _sut.ImportAsync(MakeCsv(csv));

        result.Created.Should().Be(1);
        result.Updated.Should().Be(0);
        result.Errors.Should().BeEmpty();
        _typeRepo.Verify(r => r.CreateAsync(It.Is<PlantType>(t =>
            t.Name == "Tomato" &&
            t.GrowthHabit == GrowthHabit.Vining &&
            t.DaysToMaturity == 80 &&
            t.SpacingInches == 24 &&
            t.SunPreference == SunPreference.FullSun &&
            t.IsPerennial == false)), Times.Once);
        _varietyRepo.Verify(r => r.CreateAsync(It.IsAny<PlantVariety>()), Times.Never);
    }

    [Fact]
    public async Task ImportAsync_NewTypeAndVariety_CreatesBoth()
    {
        const string csv = """
            PlantTypeName,PlantVarietyName,GrowthHabit,DaysToMaturity,SpacingInches,SunPreference,IsPerennial,Notes
            Tomato,Cherokee Purple,Vining,80,24,FullSun,false,Great heirloom
            """;

        var result = await _sut.ImportAsync(MakeCsv(csv));

        result.Created.Should().Be(2);
        _typeRepo.Verify(r => r.CreateAsync(It.Is<PlantType>(t => t.Name == "Tomato")), Times.Once);
        _varietyRepo.Verify(r => r.CreateAsync(It.Is<PlantVariety>(v =>
            v.Name == "Cherokee Purple" &&
            v.PlantTypeId == 1 &&
            v.Notes == "Great heirloom")), Times.Once);
    }

    [Fact]
    public async Task ImportAsync_ExistingType_DoesNotDuplicate()
    {
        var existingType = new PlantType { Id = 5, Name = "Pepper" };
        _typeRepo.Setup(r => r.GetByNameAsync("Pepper")).ReturnsAsync(existingType);

        const string csv = """
            PlantTypeName,PlantVarietyName,GrowthHabit,DaysToMaturity,SpacingInches,SunPreference,IsPerennial,Notes
            Pepper,Jalapeño,,,18,FullSun,false,
            """;

        var result = await _sut.ImportAsync(MakeCsv(csv));

        _typeRepo.Verify(r => r.CreateAsync(It.IsAny<PlantType>()), Times.Never);
        result.Created.Should().Be(1); // only the variety
    }

    [Fact]
    public async Task ImportAsync_ExistingVariety_UpdatesAttributes()
    {
        var existingType = new PlantType { Id = 3, Name = "Lettuce" };
        var existingVariety = new PlantVariety { Id = 20, PlantTypeId = 3, Name = "Butterhead", DaysToMaturity = 55 };
        _typeRepo.Setup(r => r.GetByNameAsync("Lettuce")).ReturnsAsync(existingType);
        _varietyRepo.Setup(r => r.GetByPlantTypeAndNameAsync(3, "Butterhead")).ReturnsAsync(existingVariety);

        const string csv = """
            PlantTypeName,PlantVarietyName,GrowthHabit,DaysToMaturity,SpacingInches,SunPreference,IsPerennial,Notes
            Lettuce,Butterhead,,60,,PartialSun,,Updated notes
            """;

        var result = await _sut.ImportAsync(MakeCsv(csv));

        result.Updated.Should().Be(1);
        _varietyRepo.Verify(r => r.UpdateAsync(It.Is<PlantVariety>(v =>
            v.Id == 20 &&
            v.DaysToMaturity == 60 &&
            v.SunPreference == SunPreference.PartialSun &&
            v.Notes == "Updated notes")), Times.Once);
    }

    [Fact]
    public async Task ImportAsync_TypeOnlyRow_UpdatesExistingTypeAttributes()
    {
        var existingType = new PlantType { Id = 2, Name = "Kale", DaysToMaturity = 50 };
        _typeRepo.Setup(r => r.GetByNameAsync("Kale")).ReturnsAsync(existingType);

        const string csv = """
            PlantTypeName,PlantVarietyName,GrowthHabit,DaysToMaturity,SpacingInches,SunPreference,IsPerennial,Notes
            Kale,,Upright,60,18,FullSun,true,
            """;

        var result = await _sut.ImportAsync(MakeCsv(csv));

        result.Updated.Should().Be(1);
        _typeRepo.Verify(r => r.UpdateAsync(It.Is<PlantType>(t =>
            t.Id == 2 &&
            t.DaysToMaturity == 60 &&
            t.GrowthHabit == GrowthHabit.Upright)), Times.Once);
    }

    [Fact]
    public async Task ImportAsync_MultipleVarietiesSameType_CreatesTypeOnce()
    {
        const string csv = """
            PlantTypeName,PlantVarietyName,GrowthHabit,DaysToMaturity,SpacingInches,SunPreference,IsPerennial,Notes
            Tomato,Roma,,75,18,FullSun,false,
            Tomato,Cherokee Purple,,80,24,FullSun,false,
            Tomato,Sun Gold,,65,18,FullSun,false,
            """;

        _typeRepo.Setup(r => r.CreateAsync(It.IsAny<PlantType>())).ReturnsAsync(1);

        var result = await _sut.ImportAsync(MakeCsv(csv));

        _typeRepo.Verify(r => r.CreateAsync(It.IsAny<PlantType>()), Times.Once);
        _varietyRepo.Verify(r => r.CreateAsync(It.IsAny<PlantVariety>()), Times.Exactly(3));
        result.Created.Should().Be(4); // 1 type + 3 varieties
    }

    [Fact]
    public async Task ImportAsync_MissingTypeName_RecordsError()
    {
        const string csv = """
            PlantTypeName,PlantVarietyName,GrowthHabit,DaysToMaturity,SpacingInches,SunPreference,IsPerennial,Notes
            ,Roma,,75,18,FullSun,false,
            """;

        var result = await _sut.ImportAsync(MakeCsv(csv));

        result.Errors.Should().HaveCount(1);
        result.Errors[0].Should().Contain("PlantTypeName");
        result.Created.Should().Be(0);
    }

    [Fact]
    public async Task ImportAsync_UnknownEnumValue_TreatsAsNull()
    {
        const string csv = """
            PlantTypeName,PlantVarietyName,GrowthHabit,DaysToMaturity,SpacingInches,SunPreference,IsPerennial,Notes
            Basil,,NotARealHabit,,,FullSun,false,
            """;

        var result = await _sut.ImportAsync(MakeCsv(csv));

        result.Errors.Should().BeEmpty();
        _typeRepo.Verify(r => r.CreateAsync(It.Is<PlantType>(t =>
            t.GrowthHabit == null &&
            t.SunPreference == SunPreference.FullSun)), Times.Once);
    }

    [Fact]
    public async Task ImportAsync_EmptyCsv_ReturnsZeroCounts()
    {
        const string csv = "PlantTypeName,PlantVarietyName,GrowthHabit,DaysToMaturity,SpacingInches,SunPreference,IsPerennial,Notes\n";

        var result = await _sut.ImportAsync(MakeCsv(csv));

        result.Created.Should().Be(0);
        result.Updated.Should().Be(0);
        result.Errors.Should().BeEmpty();
    }
}
