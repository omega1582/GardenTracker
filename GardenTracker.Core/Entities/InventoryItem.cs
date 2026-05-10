using GardenTracker.Core.Enums;

namespace GardenTracker.Core.Entities;

public class InventoryItem
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int PlantVarietyId { get; set; }
    public int? SupplierId { get; set; }
    public InventoryType Type { get; set; }
    public int QuantityPurchased { get; set; }
    public int QuantityRemaining { get; set; }
    public decimal TotalCost { get; set; }
    public DateOnly PurchaseDate { get; set; }
    public string? Notes { get; set; }

    // Populated by JOIN queries in the repository — not persisted columns
    public string? PlantVarietyName { get; set; }
    public string? PlantTypeName { get; set; }
    public string? SupplierName { get; set; }

    public User User { get; set; } = null!;
    public PlantVariety PlantVariety { get; set; } = null!;
    public Supplier? Supplier { get; set; }
    public ICollection<BedPlanting> Plantings { get; set; } = [];
}
