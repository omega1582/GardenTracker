namespace GardenTracker.Api.DTOs.Responses.PlantVarieties;

public class PlantVarietyResponse
{
    public int Id { get; set; }
    public int PlantTypeId { get; set; }
    public string PlantTypeName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? GrowthHabit { get; set; }
    public int? DaysToMaturity { get; set; }
    public int? SpacingInches { get; set; }
    public string? SunPreference { get; set; }
    public bool? IsPerennial { get; set; }
    public string? ImageUrl { get; set; }
}
