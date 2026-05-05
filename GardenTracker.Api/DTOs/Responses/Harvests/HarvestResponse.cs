using GardenTracker.Core.Enums;

namespace GardenTracker.Api.DTOs.Responses.Harvests;

public class HarvestResponse
{
    public int Id { get; set; }
    public int BedId { get; set; }
    public int SeasonId { get; set; }
    public int PlantVarietyId { get; set; }
    public string PlantVarietyName { get; set; } = string.Empty;
    public string PlantTypeName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public HarvestUnit Unit { get; set; }
    public DateOnly HarvestDate { get; set; }
    public string? Notes { get; set; }
}
