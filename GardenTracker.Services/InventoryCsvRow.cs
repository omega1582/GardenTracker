namespace GardenTracker.Services;

public class InventoryCsvRow
{
    public string PlantTypeName { get; set; } = string.Empty;
    public string PlantVarietyName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int QuantityPurchased { get; set; }
    public int QuantityRemaining { get; set; }
    public decimal TotalCost { get; set; }
    public string PurchaseDate { get; set; } = string.Empty;
    public string? SupplierName { get; set; }
    public string? SupplierType { get; set; }
    public string? Notes { get; set; }
}
