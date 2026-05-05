namespace GardenTracker.Core.Models.Reports;

public class WaterAttributionMonthResult
{
    public int Month { get; set; }
    public decimal UsageGallons { get; set; }
    public decimal TotalCost { get; set; }
    public decimal? AttributedCost { get; set; }
    public decimal? AttributedGallons { get; set; }
}
