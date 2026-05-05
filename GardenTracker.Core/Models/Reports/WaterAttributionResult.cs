namespace GardenTracker.Core.Models.Reports;

public class WaterAttributionResult
{
    public int Year { get; set; }
    public decimal? BaselineMonthlyCost { get; set; }
    public decimal? BaselineMonthlyGallons { get; set; }
    public IEnumerable<WaterAttributionMonthResult> ActiveMonths { get; set; } = [];
    public decimal TotalAttributedCost { get; set; }
    public decimal TotalAttributedGallons { get; set; }
}
