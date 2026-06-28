using GardenTracker.Core.Enums;

namespace GardenTracker.Core.Entities;

public class PlantType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public PlantCategory Category { get; set; } = PlantCategory.Other;
    public GrowthHabit? GrowthHabit { get; set; }
    public int? DaysToMaturity { get; set; }
    public int? SpacingInches { get; set; }
    public SunPreference? SunPreference { get; set; }
    public bool? IsPerennial { get; set; }

    public ICollection<PlantVariety> Varieties { get; set; } = [];
    public ICollection<MarketPrice> MarketPrices { get; set; } = [];
}
