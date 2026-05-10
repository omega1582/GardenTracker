using GardenTracker.Core.Enums;

namespace GardenTracker.Api.DTOs.Responses.Plantings;

public class PlantingResponse
{
    public int Id { get; set; }
    public int BedId { get; set; }
    public string BedName { get; set; } = string.Empty;
    public int SeasonId { get; set; }
    public int PlantVarietyId { get; set; }
    public string PlantVarietyName { get; set; } = string.Empty;
    public string PlantTypeName { get; set; } = string.Empty;
    public int? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public StartMethod StartMethod { get; set; }
    public int Quantity { get; set; }
    public decimal TotalCost { get; set; }
    public int? SourceHarvestId { get; set; }
    public string? Notes { get; set; }
    public int? InventoryItemId { get; set; }
    public int? QuantityUsedFromInventory { get; set; }
}
