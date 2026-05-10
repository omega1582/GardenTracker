using System.ComponentModel.DataAnnotations;

namespace GardenTracker.Api.DTOs.Requests.Inventory;

public class AdjustInventoryRequest
{
    [Required, Range(0, 1000000)]
    public int NewRemaining { get; set; }
}
