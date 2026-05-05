using System.ComponentModel.DataAnnotations;
using GardenTracker.Core.Enums;

namespace GardenTracker.Api.DTOs.Requests.Harvests;

public class UpdateHarvestRequest
{
    [Required, Range(0.001, 100000)]
    public decimal Quantity { get; set; }

    [Required]
    public HarvestUnit Unit { get; set; }

    [Required]
    public DateOnly HarvestDate { get; set; }

    public string? Notes { get; set; }
}
