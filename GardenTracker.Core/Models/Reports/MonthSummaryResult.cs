namespace GardenTracker.Core.Models.Reports;

public class MonthSummaryResult
{
    public int Month { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal TotalHarvestValue { get; set; }
    public decimal WaterAttribution { get; set; }
    public decimal NetCost { get; set; }
}
