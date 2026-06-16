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
    public int? InventoryItemId { get; set; }
    public int? QuantityUsedFromInventory { get; set; }
    public string? Notes { get; set; }
    public decimal? PositionX { get; set; }
    public decimal? PositionY { get; set; }
    public decimal? LayoutWidth { get; set; }
    public decimal? LayoutHeight { get; set; }

    // Populated by JOIN queries in the repository — not persisted columns
    public string? BedName { get; set; }
    public string? PlantVarietyName { get; set; }
    public string? PlantTypeName { get; set; }
    public string? SupplierName { get; set; }

    public Bed Bed { get; set; } = null!;
    public Season Season { get; set; } = null!;
    public PlantVariety PlantVariety { get; set; } = null!;
    public Supplier? Supplier { get; set; }
    public Harvest? SourceHarvest { get; set; }
    public InventoryItem? InventoryItem { get; set; }
}
