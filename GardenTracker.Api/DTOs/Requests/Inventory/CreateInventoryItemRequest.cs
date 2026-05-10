using System.ComponentModel.DataAnnotations;
using GardenTracker.Core.Enums;

namespace GardenTracker.Api.DTOs.Requests.Inventory;

public class CreateInventoryItemRequest
{
    [Required]
    public int PlantVarietyId { get; set; }

    public int? SupplierId { get; set; }

    [Required]
    public InventoryType Type { get; set; }

    [Required, Range(1, 1000000)]
    public int QuantityPurchased { get; set; }

    [Required, Range(0, 1000000)]
    public decimal TotalCost { get; set; }

    [Required]
    public DateOnly PurchaseDate { get; set; }

    public string? Notes { get; set; }
}
