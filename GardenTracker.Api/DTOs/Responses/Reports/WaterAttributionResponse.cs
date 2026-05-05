namespace GardenTracker.Api.DTOs.Responses.Reports;

public class WaterAttributionResponse
{
    public int Year { get; set; }
    public decimal? BaselineMonthlyCost { get; set; }
    public decimal? BaselineMonthlyGallons { get; set; }
    public IEnumerable<WaterAttributionMonthResponse> ActiveMonths { get; set; } = [];
    public decimal TotalAttributedCost { get; set; }
    public decimal TotalAttributedGallons { get; set; }
}

public class WaterAttributionMonthResponse
{
    public int Month { get; set; }
    public decimal UsageGallons { get; set; }
    public decimal TotalCost { get; set; }
    public decimal? AttributedCost { get; set; }
    public decimal? AttributedGallons { get; set; }
}
