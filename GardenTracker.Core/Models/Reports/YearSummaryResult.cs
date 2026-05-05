namespace GardenTracker.Core.Models.Reports;

public class YearSummaryResult
{
    public int Year { get; set; }
    public IEnumerable<MonthSummaryResult> Months { get; set; } = [];
    public decimal TotalExpenses { get; set; }
    public decimal TotalHarvestValue { get; set; }
    public decimal WaterAttribution { get; set; }
    public decimal NetCost { get; set; }
}
