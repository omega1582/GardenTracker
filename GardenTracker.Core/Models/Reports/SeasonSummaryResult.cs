namespace GardenTracker.Core.Models.Reports;

public class SeasonSummaryResult
{
    public int GardenId { get; set; }
    public int Year { get; set; }
    public IEnumerable<SeasonExpenseTotal> ExpensesByCategory { get; set; } = [];
    public decimal TotalExpenses { get; set; }
    public IEnumerable<HarvestValueLine> HarvestLines { get; set; } = [];
    public decimal TotalHarvestValue { get; set; }
    public decimal WaterAttribution { get; set; }
    public decimal NetCost { get; set; }
}
