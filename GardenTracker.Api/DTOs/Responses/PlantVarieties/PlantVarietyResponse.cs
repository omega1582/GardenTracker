namespace GardenTracker.Api.DTOs.Responses.PlantVarieties;

public class PlantVarietyResponse
{
    public int Id { get; set; }
    public int PlantTypeId { get; set; }
    public string PlantTypeName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
