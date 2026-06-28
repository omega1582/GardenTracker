namespace GardenTracker.Api.DTOs.Responses.PlantTypes;

public class PlantTypeResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = "Other";
    public string? GrowthHabit { get; set; }
    public int? DaysToMaturity { get; set; }
    public int? SpacingInches { get; set; }
    public string? SunPreference { get; set; }
    public bool? IsPerennial { get; set; }
}
