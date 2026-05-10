using GardenTracker.Core.Enums;

namespace GardenTracker.Api.DTOs.Responses.Inventory;

public class InventoryItemResponse
{
    public int Id { get; set; }
    public int PlantVarietyId { get; set; }
    public string PlantVarietyName { get; set; } = string.Empty;
    public string PlantTypeName { get; set; } = string.Empty;
    public int? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public InventoryType Type { get; set; }
    public int QuantityPurchased { get; set; }
    public int QuantityRemaining { get; set; }
    public decimal TotalCost { get; set; }
    public DateOnly PurchaseDate { get; set; }
    public string? Notes { get; set; }
}
