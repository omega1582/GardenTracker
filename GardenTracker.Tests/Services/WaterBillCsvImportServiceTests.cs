using System.Text;
using FluentAssertions;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Interfaces.Repositories;
using GardenTracker.Services;
using Moq;

namespace GardenTracker.Tests.Services;

public class WaterBillCsvImportServiceTests
{
    private readonly Mock<IWaterBillRepository> _repo = new();
    private readonly WaterBillCsvImportService _sut;

    public WaterBillCsvImportServiceTests()
    {
        _sut = new WaterBillCsvImportService(_repo.Object);
        _repo.Setup(r => r.GetByYearMonthAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
             .ReturnsAsync((WaterBill?)null);
        _repo.Setup(r => r.CreateAsync(It.IsAny<WaterBill>())).ReturnsAsync(1);
    }

    private static Stream MakeCsv(string content) =>
        new MemoryStream(Encoding.UTF8.GetBytes(content));

    [Fact]
    public async Task ImportAsync_ValidRow_CreatesNewBill()
    {
        const string csv = """
            Year,Month,UsageCcf,UsageGallons,TotalCost,IsGardenActive,Notes
            2025,6,8.5,6360,94.50,true,Summer watering
            """;

        var result = await _sut.ImportAsync(1, MakeCsv(csv));

        result.Created.Should().Be(1);
        result.Updated.Should().Be(0);
        result.Errors.Should().BeEmpty();
        _repo.Verify(r => r.CreateAsync(It.Is<WaterBill>(b =>
            b.UserId == 1 &&
            b.Year == 2025 &&
            b.Month == 6 &&
            b.UsageCcf == 8.5m &&
            b.TotalCost == 94.50m &&
            b.IsGardenActive == true &&
            b.Notes == "Summer watering")), Times.Once);
    }

    [Fact]
    public async Task ImportAsync_MultipleRows_CreatesAll()
    {
        const string csv = """
            Year,Month,UsageCcf,UsageGallons,TotalCost,IsGardenActive,Notes
            2025,1,4.2,3141,52.00,false,
            2025,2,4.0,2992,50.50,false,
            2025,6,9.1,6806,98.00,true,
            """;

        var result = await _sut.ImportAsync(1, MakeCsv(csv));

        result.Created.Should().Be(3);
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ImportAsync_ExistingBill_UpdatesInsteadOfCreating()
    {
        var existing = new WaterBill
        {
            Id = 10, UserId = 1, Year = 2025, Month = 3,
            UsageCcf = 5.0m, UsageGallons = 3740m, TotalCost = 60.00m,
            IsGardenActive = false
        };
        _repo.Setup(r => r.GetByYearMonthAsync(1, 2025, 3)).ReturnsAsync(existing);

        const string csv = """
            Year,Month,UsageCcf,UsageGallons,TotalCost,IsGardenActive,Notes
            2025,3,5.5,4114,65.00,true,Updated
            """;

        var result = await _sut.ImportAsync(1, MakeCsv(csv));

        result.Updated.Should().Be(1);
        result.Created.Should().Be(0);
        _repo.Verify(r => r.CreateAsync(It.IsAny<WaterBill>()), Times.Never);
        _repo.Verify(r => r.UpdateAsync(It.Is<WaterBill>(b =>
            b.Id == 10 &&
            b.UsageCcf == 5.5m &&
            b.TotalCost == 65.00m &&
            b.IsGardenActive == true &&
            b.Notes == "Updated")), Times.Once);
    }

    [Fact]
    public async Task ImportAsync_InvalidMonth_RecordsErrorAndContinues()
    {
        const string csv = """
            Year,Month,UsageCcf,UsageGallons,TotalCost,IsGardenActive,Notes
            2025,13,4.0,2992,50.00,false,
            2025,5,6.0,4488,72.00,true,
            """;

        var result = await _sut.ImportAsync(1, MakeCsv(csv));

        result.Errors.Should().HaveCount(1);
        result.Errors[0].Should().Contain("month");
        result.Created.Should().Be(1);
    }

    [Fact]
    public async Task ImportAsync_InvalidYear_RecordsError()
    {
        const string csv = """
            Year,Month,UsageCcf,UsageGallons,TotalCost,IsGardenActive,Notes
            1899,6,4.0,2992,50.00,false,
            """;

        var result = await _sut.ImportAsync(1, MakeCsv(csv));

        result.Errors.Should().HaveCount(1);
        result.Errors[0].Should().Contain("year");
        result.Created.Should().Be(0);
    }

    [Fact]
    public async Task ImportAsync_EmptyCsv_ReturnsZeroCounts()
    {
        const string csv = "Year,Month,UsageCcf,UsageGallons,TotalCost,IsGardenActive,Notes\n";

        var result = await _sut.ImportAsync(1, MakeCsv(csv));

        result.Created.Should().Be(0);
        result.Updated.Should().Be(0);
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ImportAsync_ExistingBill_PreservesNotesWhenImportNoteIsEmpty()
    {
        var existing = new WaterBill
        {
            Id = 5, UserId = 1, Year = 2025, Month = 4,
            UsageCcf = 5.0m, UsageGallons = 3740m, TotalCost = 60.00m,
            IsGardenActive = false, Notes = "Original note"
        };
        _repo.Setup(r => r.GetByYearMonthAsync(1, 2025, 4)).ReturnsAsync(existing);

        const string csv = """
            Year,Month,UsageCcf,UsageGallons,TotalCost,IsGardenActive,Notes
            2025,4,5.0,3740,60.00,false,
            """;

        await _sut.ImportAsync(1, MakeCsv(csv));

        _repo.Verify(r => r.UpdateAsync(It.Is<WaterBill>(b => b.Notes == "Original note")), Times.Once);
    }
}
