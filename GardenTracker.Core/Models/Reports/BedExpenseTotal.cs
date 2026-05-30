namespace GardenTracker.Core.Models.Reports;

public class BedExpenseTotal
{
    public int BedId { get; set; }
    public string BedName { get; set; } = string.Empty;
    public decimal Total { get; set; }
}
