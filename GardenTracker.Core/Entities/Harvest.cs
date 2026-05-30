using GardenTracker.Core.Enums;

namespace GardenTracker.Core.Entities;

public class Harvest
{
    public int Id { get; set; }
    public int BedId { get; set; }
    public int SeasonId { get; set; }
    public int PlantVarietyId { get; set; }
    public decimal Quantity { get; set; }
    public HarvestUnit Unit { get; set; }
    public DateOnly HarvestDate { get; set; }
    public string? Notes { get; set; }

    public Bed Bed { get; set; } = null!;
    public Season Season { get; set; } = null!;
    public PlantVariety PlantVariety { get; set; } = null!;
    public ICollection<BedPlanting> SeedSavedPlantings { get; set; } = [];

    // Populated by Dapper JOINs — not persisted
    public string? BedName { get; set; }
    public string? PlantVarietyName { get; set; }
    public string? PlantTypeName { get; set; }
}
