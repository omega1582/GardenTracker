using GardenTracker.Core.Entities;
using GardenTracker.Core.Interfaces.Repositories;
using GardenTracker.Core.Interfaces.Services;
using GardenTracker.Core.Models.Reports;
using Microsoft.Extensions.Logging;

namespace GardenTracker.Services;

public class ReportService(
    IReportRepository reportRepository,
    IWaterBillRepository waterBillRepository,
    IGardenRepository gardenRepository,
    ILogger<ReportService> logger) : IReportService
{
    public async Task<SeasonSummaryResult?> GetSeasonSummaryAsync(int gardenId, int year, int userId)
    {
        var garden = await gardenRepository.GetByIdAsync(gardenId);
        if (garden?.UserId != userId)
        {
            logger.LogInformation("Season summary for garden {GardenId} year {Year} denied — not owned by user {UserId}", gardenId, year, userId);
            return null;
        }

        var expenseTotals = (await reportRepository.GetSeasonExpenseTotalsAsync(gardenId, year)).ToList();
        var harvestLines = (await reportRepository.GetSeasonHarvestValuesAsync(gardenId, year)).ToList();
        var waterBills = (await waterBillRepository.GetByUserAsync(userId, year)).ToList();

        var totalExpenses = expenseTotals.Sum(e => e.Total);
        var totalHarvestValue = harvestLines
            .Where(h => h.PricePerUnit.HasValue)
            .Sum(h => h.Quantity * h.PricePerUnit!.Value);
        var waterAttribution = ComputeWaterAttributionTotal(waterBills);

        logger.LogInformation("Season summary generated for garden {GardenId} year {Year} — expenses {TotalExpenses:C}, harvest {TotalHarvestValue:C}", gardenId, year, totalExpenses, totalHarvestValue);

        return new SeasonSummaryResult
        {
            GardenId = gardenId,
            Year = year,
            ExpensesByCategory = expenseTotals,
            TotalExpenses = totalExpenses,
            HarvestLines = harvestLines,
            TotalHarvestValue = totalHarvestValue,
            WaterAttribution = waterAttribution,
            NetCost = totalExpenses + waterAttribution - totalHarvestValue
        };
    }

    public async Task<IEnumerable<WaterAttributionResult>> GetWaterAttributionAsync(int userId, int? year)
    {
        var bills = (await waterBillRepository.GetByUserAsync(userId, year)).ToList();
        logger.LogInformation("Water attribution report generated for user {UserId} — {BillCount} bills across {YearCount} year(s)", userId, bills.Count, bills.Select(b => b.Year).Distinct().Count());

        return bills
            .GroupBy(b => b.Year)
            .Select(g => BuildWaterAttributionResult(g.Key, g.ToList()))
            .OrderByDescending(r => r.Year);
    }

    public async Task<IEnumerable<YearSummaryResult>?> GetYearOverYearAsync(int gardenId, int userId)
    {
        var garden = await gardenRepository.GetByIdAsync(gardenId);
        if (garden?.UserId != userId)
        {
            logger.LogInformation("Year-over-year report for garden {GardenId} denied — not owned by user {UserId}", gardenId, userId);
            return null;
        }

        var years = (await reportRepository.GetSeasonYearsAsync(gardenId)).ToList();
        var allWaterBills = (await waterBillRepository.GetByUserAsync(userId)).ToList();
        logger.LogInformation("Year-over-year report generated for garden {GardenId} — {YearCount} season(s)", gardenId, years.Count);

        var results = new List<YearSummaryResult>();
        foreach (var year in years)
        {
            var monthlyExpenses = (await reportRepository.GetMonthlyExpenseTotalsAsync(gardenId, year))
                .ToDictionary(m => m.Month, m => m.Total);
            var monthlyHarvests = (await reportRepository.GetMonthlyHarvestValueTotalsAsync(gardenId, year))
                .ToDictionary(m => m.Month, m => m.Total);
            var yearBills = allWaterBills.Where(b => b.Year == year).ToList();
            var monthlyWater = ComputeMonthlyWaterAttribution(yearBills);

            var allMonths = monthlyExpenses.Keys
                .Union(monthlyHarvests.Keys)
                .Union(monthlyWater.Keys)
                .OrderBy(m => m);

            var months = allMonths.Select(m =>
            {
                var expenses = monthlyExpenses.GetValueOrDefault(m);
                var harvests = monthlyHarvests.GetValueOrDefault(m);
                var water = monthlyWater.GetValueOrDefault(m);
                return new MonthSummaryResult
                {
                    Month = m,
                    TotalExpenses = expenses,
                    TotalHarvestValue = harvests,
                    WaterAttribution = water,
                    NetCost = expenses + water - harvests
                };
            }).ToList();

            var totalExpenses = months.Sum(m => m.TotalExpenses);
            var totalHarvestValue = months.Sum(m => m.TotalHarvestValue);
            var totalWater = months.Sum(m => m.WaterAttribution);

            results.Add(new YearSummaryResult
            {
                Year = year,
                Months = months,
                TotalExpenses = totalExpenses,
                TotalHarvestValue = totalHarvestValue,
                WaterAttribution = totalWater,
                NetCost = totalExpenses + totalWater - totalHarvestValue
            });
        }

        return results;
    }

    public async Task<IEnumerable<BedBreakdownResult>?> GetBedBreakdownAsync(int gardenId, int year, int userId)
    {
        var garden = await gardenRepository.GetByIdAsync(gardenId);
        if (garden?.UserId != userId) return null;

        var bedExpenses = (await reportRepository.GetBedExpenseTotalsAsync(gardenId, year))
            .ToDictionary(b => b.BedId);
        var bedHarvests = (await reportRepository.GetBedHarvestLinesAsync(gardenId, year))
            .GroupBy(h => h.BedId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var allBedIds = bedExpenses.Keys.Union(bedHarvests.Keys).ToHashSet();

        return allBedIds.Select(bedId =>
        {
            var expenseRow = bedExpenses.GetValueOrDefault(bedId);
            var harvests = bedHarvests.GetValueOrDefault(bedId) ?? [];
            var harvestValue = harvests
                .Where(h => h.PricePerUnit.HasValue)
                .Sum(h => h.Quantity * h.PricePerUnit!.Value);
            var expenses = expenseRow?.Total ?? 0;
            return new BedBreakdownResult
            {
                BedId = bedId,
                BedName = expenseRow?.BedName ?? harvests.FirstOrDefault()?.BedName ?? string.Empty,
                TotalExpenses = expenses,
                HarvestLines = harvests,
                TotalHarvestValue = harvestValue,
                NetCost = expenses - harvestValue
            };
        }).OrderBy(b => b.BedName);
    }

    private static Dictionary<int, decimal> ComputeMonthlyWaterAttribution(List<WaterBill> bills)
    {
        var baselineMonths = bills.Where(b => !b.IsGardenActive).ToList();
        if (baselineMonths.Count == 0) return [];

        var baselineMonthlyCost = baselineMonths.Average(b => b.TotalCost);
        return bills
            .Where(b => b.IsGardenActive)
            .ToDictionary(b => b.Month, b => Math.Max(0, b.TotalCost - baselineMonthlyCost));
    }

    private static decimal ComputeWaterAttributionTotal(List<WaterBill> bills) =>
        ComputeMonthlyWaterAttribution(bills).Values.Sum();

    private static WaterAttributionResult BuildWaterAttributionResult(int year, List<WaterBill> bills)
    {
        var baselineMonths = bills.Where(b => !b.IsGardenActive).ToList();
        var activeMonths = bills.Where(b => b.IsGardenActive).ToList();

        decimal? baselineMonthlyCost = baselineMonths.Count > 0 ? baselineMonths.Average(b => b.TotalCost) : null;
        decimal? baselineMonthlyGallons = baselineMonths.Count > 0 ? baselineMonths.Average(b => b.UsageGallons) : null;

        var monthDetails = activeMonths.Select(b => new WaterAttributionMonthResult
        {
            Month = b.Month,
            UsageGallons = b.UsageGallons,
            TotalCost = b.TotalCost,
            AttributedCost = baselineMonthlyCost.HasValue ? Math.Max(0, b.TotalCost - baselineMonthlyCost.Value) : null,
            AttributedGallons = baselineMonthlyGallons.HasValue ? Math.Max(0, b.UsageGallons - baselineMonthlyGallons.Value) : null
        }).ToList();

        return new WaterAttributionResult
        {
            Year = year,
            BaselineMonthlyCost = baselineMonthlyCost,
            BaselineMonthlyGallons = baselineMonthlyGallons,
            ActiveMonths = monthDetails,
            TotalAttributedCost = monthDetails.Sum(m => m.AttributedCost ?? 0),
            TotalAttributedGallons = monthDetails.Sum(m => m.AttributedGallons ?? 0)
        };
    }
}
