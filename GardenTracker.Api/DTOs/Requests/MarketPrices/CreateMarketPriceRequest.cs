using System.ComponentModel.DataAnnotations;
using GardenTracker.Core.Enums;

namespace GardenTracker.Api.DTOs.Requests.MarketPrices;

public class CreateMarketPriceRequest
{
    [Required]
    public int PlantTypeId { get; set; }

    public int? PlantVarietyId { get; set; }

    [Required, Range(0.01, 10000)]
    public decimal PricePerUnit { get; set; }

    [Required]
    public HarvestUnit Unit { get; set; }

    [Required]
    public DateOnly RecordedDate { get; set; }
}
