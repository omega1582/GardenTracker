namespace GardenTracker.Services;

public class PlantCatalogCsvRow
{
    public string PlantTypeName { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? PlantVarietyName { get; set; }
    public string? GrowthHabit { get; set; }
    public int? DaysToMaturity { get; set; }
    public int? SpacingInches { get; set; }
    public string? SunPreference { get; set; }
    public bool? IsPerennial { get; set; }
    public string? Notes { get; set; }
}
