using FluentAssertions;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Enums;
using GardenTracker.Core.Interfaces.Repositories;
using GardenTracker.Core.Models.Reports;
using GardenTracker.Services;
using Moq;

namespace GardenTracker.Tests.Services;

public class ReportServiceTests
{
    private readonly Mock<IReportRepository> _reportRepo = new();
    private readonly Mock<IWaterBillRepository> _waterBillRepo = new();
    private readonly Mock<IGardenRepository> _gardenRepo = new();
    private readonly ReportService _sut;

    public ReportServiceTests() =>
        _sut = new ReportService(_reportRepo.Object, _waterBillRepo.Object, _gardenRepo.Object);

    // --- GetSeasonSummaryAsync ---

    [Fact]
    public async Task GetSeasonSummaryAsync_ReturnsNull_WhenUserDoesNotOwnGarden()
    {
        _gardenRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Garden { Id = 1, UserId = 42 });

        var result = await _sut.GetSeasonSummaryAsync(gardenId: 1, year: 2025, userId: 99);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetSeasonSummaryAsync_ComputesTotalsAndNetCost()
    {
        _gardenRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Garden { Id = 1, UserId = 42 });
        _reportRepo.Setup(r => r.GetSeasonExpenseTotalsAsync(1, 2025)).ReturnsAsync(
        [
            new SeasonExpenseTotal { Category = ExpenseCategory.Seeds, Total = 50m },
            new SeasonExpenseTotal { Category = ExpenseCategory.Soil, Total = 100m }
        ]);
        _reportRepo.Setup(r => r.GetSeasonHarvestValuesAsync(1, 2025)).ReturnsAsync(
        [
            new HarvestValueLine { Quantity = 10m, Unit = HarvestUnit.Pounds, PricePerUnit = 2.00m },
            new HarvestValueLine { Quantity = 5m,  Unit = HarvestUnit.Pounds, PricePerUnit = null }  // no price — excluded from total
        ]);
        _waterBillRepo.Setup(r => r.GetByUserAsync(42, 2025)).ReturnsAsync(
        [
            new WaterBill { Month = 1, TotalCost = 40m, UsageGallons = 2000m, IsGardenActive = false },
            new WaterBill { Month = 6, TotalCost = 70m, UsageGallons = 3500m, IsGardenActive = true }
        ]);

        var result = await _sut.GetSeasonSummaryAsync(1, 2025, userId: 42);

        result.Should().NotBeNull();
        result!.TotalExpenses.Should().Be(150m);
        result.TotalHarvestValue.Should().Be(20m);      // 10 * 2.00 only
        result.WaterAttribution.Should().Be(30m);       // 70 - 40 baseline
        result.NetCost.Should().Be(160m);               // 150 + 30 - 20
    }

    // --- GetWaterAttributionAsync ---

    [Fact]
    public async Task GetWaterAttributionAsync_ComputesBaselineAndClampsNegativeAttribution()
    {
        _waterBillRepo.Setup(r => r.GetByUserAsync(42, null)).ReturnsAsync(
        [
            new WaterBill { Year = 2025, Month = 1, TotalCost = 40m, UsageGallons = 2000m, IsGardenActive = false },
            new WaterBill { Year = 2025, Month = 2, TotalCost = 40m, UsageGallons = 2000m, IsGardenActive = false },
            new WaterBill { Year = 2025, Month = 6, TotalCost = 80m, UsageGallons = 4000m, IsGardenActive = true },
            new WaterBill { Year = 2025, Month = 7, TotalCost = 30m, UsageGallons = 1500m, IsGardenActive = true }  // below baseline
        ]);

        var results = (await _sut.GetWaterAttributionAsync(42, year: null)).ToList();

        results.Should().HaveCount(1);
        var year = results[0];
        year.BaselineMonthlyCost.Should().Be(40m);
        year.TotalAttributedCost.Should().Be(40m);      // (80-40) + max(0, 30-40) = 40 + 0
        var julyMonth = year.ActiveMonths.Single(m => m.Month == 7);
        julyMonth.AttributedCost.Should().Be(0m);       // clamped, not negative
    }

    [Fact]
    public async Task GetWaterAttributionAsync_ReturnsNullBaseline_WhenAllMonthsAreGardenActive()
    {
        _waterBillRepo.Setup(r => r.GetByUserAsync(42, null)).ReturnsAsync(
        [
            new WaterBill { Year = 2025, Month = 6, TotalCost = 80m, UsageGallons = 4000m, IsGardenActive = true },
            new WaterBill { Year = 2025, Month = 7, TotalCost = 90m, UsageGallons = 4500m, IsGardenActive = true }
        ]);

        var results = (await _sut.GetWaterAttributionAsync(42, year: null)).ToList();

        results[0].BaselineMonthlyCost.Should().BeNull();
        results[0].TotalAttributedCost.Should().Be(0m);
        results[0].ActiveMonths.Should().AllSatisfy(m => m.AttributedCost.Should().BeNull());
    }

    [Fact]
    public async Task GetWaterAttributionAsync_GroupsByYear()
    {
        _waterBillRepo.Setup(r => r.GetByUserAsync(42, null)).ReturnsAsync(
        [
            new WaterBill { Year = 2024, Month = 1, TotalCost = 35m, IsGardenActive = false },
            new WaterBill { Year = 2025, Month = 1, TotalCost = 40m, IsGardenActive = false }
        ]);

        var results = (await _sut.GetWaterAttributionAsync(42, year: null)).ToList();

        results.Should().HaveCount(2);
        results.Select(r => r.Year).Should().BeEquivalentTo([2025, 2024]);
    }

    // --- GetYearOverYearAsync ---

    [Fact]
    public async Task GetYearOverYearAsync_ReturnsNull_WhenUserDoesNotOwnGarden()
    {
        _gardenRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Garden { Id = 1, UserId = 42 });

        var result = await _sut.GetYearOverYearAsync(gardenId: 1, userId: 99);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetYearOverYearAsync_ReturnsMonthlyBreakdownWithCorrectTotals()
    {
        _gardenRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Garden { Id = 1, UserId = 42 });
        _reportRepo.Setup(r => r.GetSeasonYearsAsync(1)).ReturnsAsync([2025]);
        _waterBillRepo.Setup(r => r.GetByUserAsync(42, null)).ReturnsAsync(
        [
            new WaterBill { Year = 2025, Month = 1, TotalCost = 40m, UsageGallons = 2000m, IsGardenActive = false },
            new WaterBill { Year = 2025, Month = 6, TotalCost = 70m, UsageGallons = 3500m, IsGardenActive = true }
        ]);
        _reportRepo.Setup(r => r.GetMonthlyExpenseTotalsAsync(1, 2025)).ReturnsAsync(
        [
            new MonthlyTotal { Month = 4, Total = 150m },
            new MonthlyTotal { Month = 6, Total = 20m }
        ]);
        _reportRepo.Setup(r => r.GetMonthlyHarvestValueTotalsAsync(1, 2025)).ReturnsAsync(
        [
            new MonthlyTotal { Month = 7, Total = 85m }
        ]);

        var results = (await _sut.GetYearOverYearAsync(1, userId: 42))!.ToList();

        results.Should().HaveCount(1);
        var year = results[0];
        year.Year.Should().Be(2025);
        year.Months.Should().HaveCount(3);              // months 4, 6, 7
        year.TotalExpenses.Should().Be(170m);           // 150 + 20
        year.TotalHarvestValue.Should().Be(85m);
        year.WaterAttribution.Should().Be(30m);         // 70 - 40 baseline
        year.NetCost.Should().Be(115m);                 // 170 + 30 - 85
    }
}
