using System.ComponentModel.DataAnnotations;
using GardenTracker.Core.Enums;

namespace GardenTracker.Api.DTOs.Requests.Plantings;

public class UpdatePlantingRequest
{
    public int? SupplierId { get; set; }

    [Required]
    public StartMethod StartMethod { get; set; }

    [Required, Range(1, 10000)]
    public int Quantity { get; set; }

    [Required, Range(0, 100000)]
    public decimal TotalCost { get; set; }

    public int? SourceHarvestId { get; set; }

    public string? Notes { get; set; }
}
