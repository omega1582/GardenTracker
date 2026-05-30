using GardenTracker.Core.Enums;

namespace GardenTracker.Core.Entities;

public class MarketPrice
{
    public int Id { get; set; }
    public int SeasonId { get; set; }
    public int PlantTypeId { get; set; }
    public int? PlantVarietyId { get; set; }
    public decimal PricePerUnit { get; set; }
    public HarvestUnit Unit { get; set; }
    public DateOnly RecordedDate { get; set; }

    public Season Season { get; set; } = null!;
    public PlantType PlantType { get; set; } = null!;
    public PlantVariety? PlantVariety { get; set; }

    // Populated by Dapper JOINs — not persisted
    public string? PlantTypeName { get; set; }
    public string? PlantVarietyName { get; set; }
}
