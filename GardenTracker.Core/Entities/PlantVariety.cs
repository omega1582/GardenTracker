namespace GardenTracker.Core.Entities;

public class PlantVariety
{
    public int Id { get; set; }
    public int PlantTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Notes { get; set; }

    public PlantType PlantType { get; set; } = null!;
    public ICollection<BedPlanting> Plantings { get; set; } = [];
    public ICollection<Harvest> Harvests { get; set; } = [];
    public ICollection<MarketPrice> MarketPrices { get; set; } = [];
}
