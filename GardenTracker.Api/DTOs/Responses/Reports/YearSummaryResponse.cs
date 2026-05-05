namespace GardenTracker.Api.DTOs.Responses.Reports;

public class YearSummaryResponse
{
    public int Year { get; set; }
    public IEnumerable<MonthSummaryResponse> Months { get; set; } = [];
    public decimal TotalExpenses { get; set; }
    public decimal TotalHarvestValue { get; set; }
    public decimal WaterAttribution { get; set; }
    public decimal NetCost { get; set; }
}

public class MonthSummaryResponse
{
    public int Month { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal TotalHarvestValue { get; set; }
    public decimal WaterAttribution { get; set; }
    public decimal NetCost { get; set; }
}
