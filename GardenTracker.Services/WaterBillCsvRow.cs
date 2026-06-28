namespace GardenTracker.Services;

public class WaterBillCsvRow
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal UsageCcf { get; set; }
    public decimal UsageGallons { get; set; }
    public decimal TotalCost { get; set; }
    public bool IsGardenActive { get; set; }
    public string? Notes { get; set; }
}
