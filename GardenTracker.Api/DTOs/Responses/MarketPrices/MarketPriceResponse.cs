using GardenTracker.Core.Enums;

namespace GardenTracker.Api.DTOs.Responses.MarketPrices;

public class MarketPriceResponse
{
    public int Id { get; set; }
    public int SeasonId { get; set; }
    public int PlantTypeId { get; set; }
    public string PlantTypeName { get; set; } = string.Empty;
    public int? PlantVarietyId { get; set; }
    public string? PlantVarietyName { get; set; }
    public decimal PricePerUnit { get; set; }
    public HarvestUnit Unit { get; set; }
    public DateOnly RecordedDate { get; set; }
}
