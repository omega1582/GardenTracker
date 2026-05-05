using GardenTracker.Core.Enums;

namespace GardenTracker.Api.DTOs.Responses.Reports;

public class SeasonSummaryResponse
{
    public int GardenId { get; set; }
    public int Year { get; set; }
    public Dictionary<string, decimal> ExpensesByCategory { get; set; } = [];
    public decimal TotalExpenses { get; set; }
    public IEnumerable<HarvestLineResponse> HarvestLines { get; set; } = [];
    public decimal TotalHarvestValue { get; set; }
    public decimal WaterAttribution { get; set; }
    public decimal NetCost { get; set; }
}

public class HarvestLineResponse
{
    public string VarietyName { get; set; } = string.Empty;
    public string PlantTypeName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public HarvestUnit Unit { get; set; }
    public decimal? PricePerUnit { get; set; }
    public decimal? Value { get; set; }
}
