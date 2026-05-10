using System.ComponentModel.DataAnnotations;

namespace GardenTracker.Api.DTOs.Requests.Inventory;

public class UpdateInventoryItemRequest
{
    public int? SupplierId { get; set; }

    [Required, Range(1, 1000000)]
    public int QuantityPurchased { get; set; }

    [Required, Range(0, 1000000)]
    public decimal TotalCost { get; set; }

    [Required]
    public DateOnly PurchaseDate { get; set; }

    public string? Notes { get; set; }
}
