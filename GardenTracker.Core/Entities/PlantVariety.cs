using GardenTracker.Core.Enums;

namespace GardenTracker.Core.Entities;

public class PlantVariety
{
    public int Id { get; set; }
    public int PlantTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public GrowthHabit? GrowthHabit { get; set; }
    public int? DaysToMaturity { get; set; }
    public int? SpacingInches { get; set; }
    public SunPreference? SunPreference { get; set; }
    public bool? IsPerennial { get; set; }

    public PlantType PlantType { get; set; } = null!;
    public ICollection<BedPlanting> Plantings { get; set; } = [];
    public ICollection<Harvest> Harvests { get; set; } = [];
    public ICollection<MarketPrice> MarketPrices { get; set; } = [];
}
