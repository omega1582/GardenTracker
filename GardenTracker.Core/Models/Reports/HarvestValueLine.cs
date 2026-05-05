using GardenTracker.Core.Enums;

namespace GardenTracker.Core.Models.Reports;

public class HarvestValueLine
{
    public string VarietyName { get; set; } = string.Empty;
    public string PlantTypeName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public HarvestUnit Unit { get; set; }
    public decimal? PricePerUnit { get; set; }
}
