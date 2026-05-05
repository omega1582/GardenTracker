using System.ComponentModel.DataAnnotations;
using GardenTracker.Core.Enums;

namespace GardenTracker.Api.DTOs.Requests.Harvests;

public class CreateHarvestRequest
{
    [Required]
    public int BedId { get; set; }

    [Required]
    public int PlantVarietyId { get; set; }

    [Required, Range(0.001, 100000)]
    public decimal Quantity { get; set; }

    [Required]
    public HarvestUnit Unit { get; set; }

    [Required]
    public DateOnly HarvestDate { get; set; }

    public string? Notes { get; set; }
}
