using GardenTracker.Core.Enums;

namespace GardenTracker.Core.Entities;

public class BedPlanting
{
    public int Id { get; set; }
    public int BedId { get; set; }
    public int SeasonId { get; set; }
    public int PlantVarietyId { get; set; }
    public int? SupplierId { get; set; }
    public StartMethod StartMethod { get; set; }
    public int Quantity { get; set; }
    public decimal TotalCost { get; set; }
    public int? SourceHarvestId { get; set; }
    public string? Notes { get; set; }

    public RaisedBed Bed { get; set; } = null!;
    public Season Season { get; set; } = null!;
    public PlantVariety PlantVariety { get; set; } = null!;
    public Supplier? Supplier { get; set; }
    public Harvest? SourceHarvest { get; set; }
}
