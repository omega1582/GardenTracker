namespace GardenTracker.Core.Models.Reports;

public class BedHarvestLine : HarvestValueLine
{
    public int BedId { get; set; }
    public string BedName { get; set; } = string.Empty;
}
