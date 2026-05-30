namespace GardenTracker.Core.Models.Reports;

public class BedBreakdownResult
{
    public int BedId { get; set; }
    public string BedName { get; set; } = string.Empty;
    public decimal TotalExpenses { get; set; }
    public IEnumerable<HarvestValueLine> HarvestLines { get; set; } = [];
    public decimal TotalHarvestValue { get; set; }
    public decimal NetCost { get; set; }
}
