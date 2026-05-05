using GardenTracker.Api.DTOs.Responses.Reports;
using GardenTracker.Core.Interfaces.Services;
using GardenTracker.Core.Models.Reports;
using Microsoft.AspNetCore.Mvc;

namespace GardenTracker.Api.Controllers;

public class ReportsController(IReportService reportService) : ApiControllerBase
{
    [HttpGet("gardens/{gardenId:int}/season/{year:int}")]
    public async Task<ActionResult<SeasonSummaryResponse>> GetSeasonSummary(int gardenId, int year)
    {
        var result = await reportService.GetSeasonSummaryAsync(gardenId, year, CurrentUserId);
        return result == null ? NotFound() : Ok(MapSeasonSummary(result));
    }

    [HttpGet("water-attribution")]
    public async Task<ActionResult<IEnumerable<WaterAttributionResponse>>> GetWaterAttribution([FromQuery] int? year)
    {
        var results = await reportService.GetWaterAttributionAsync(CurrentUserId, year);
        return Ok(results.Select(MapWaterAttribution));
    }

    [HttpGet("gardens/{gardenId:int}/year-over-year")]
    public async Task<ActionResult<IEnumerable<YearSummaryResponse>>> GetYearOverYear(int gardenId)
    {
        var results = await reportService.GetYearOverYearAsync(gardenId, CurrentUserId);
        return results == null ? NotFound() : Ok(results.Select(MapYearSummary));
    }

    private static SeasonSummaryResponse MapSeasonSummary(SeasonSummaryResult r) => new()
    {
        GardenId = r.GardenId,
        Year = r.Year,
        ExpensesByCategory = r.ExpensesByCategory.ToDictionary(e => e.Category.ToString(), e => e.Total),
        TotalExpenses = r.TotalExpenses,
        HarvestLines = r.HarvestLines.Select(h => new HarvestLineResponse
        {
            VarietyName = h.VarietyName,
            PlantTypeName = h.PlantTypeName,
            Quantity = h.Quantity,
            Unit = h.Unit,
            PricePerUnit = h.PricePerUnit,
            Value = h.PricePerUnit.HasValue ? h.Quantity * h.PricePerUnit.Value : null
        }),
        TotalHarvestValue = r.TotalHarvestValue,
        WaterAttribution = r.WaterAttribution,
        NetCost = r.NetCost
    };

    private static WaterAttributionResponse MapWaterAttribution(WaterAttributionResult r) => new()
    {
        Year = r.Year,
        BaselineMonthlyCost = r.BaselineMonthlyCost,
        BaselineMonthlyGallons = r.BaselineMonthlyGallons,
        ActiveMonths = r.ActiveMonths.Select(m => new WaterAttributionMonthResponse
        {
            Month = m.Month,
            UsageGallons = m.UsageGallons,
            TotalCost = m.TotalCost,
            AttributedCost = m.AttributedCost,
            AttributedGallons = m.AttributedGallons
        }),
        TotalAttributedCost = r.TotalAttributedCost,
        TotalAttributedGallons = r.TotalAttributedGallons
    };

    private static YearSummaryResponse MapYearSummary(YearSummaryResult r) => new()
    {
        Year = r.Year,
        Months = r.Months.Select(m => new MonthSummaryResponse
        {
            Month = m.Month,
            TotalExpenses = m.TotalExpenses,
            TotalHarvestValue = m.TotalHarvestValue,
            WaterAttribution = m.WaterAttribution,
            NetCost = m.NetCost
        }),
        TotalExpenses = r.TotalExpenses,
        TotalHarvestValue = r.TotalHarvestValue,
        WaterAttribution = r.WaterAttribution,
        NetCost = r.NetCost
    };
}
