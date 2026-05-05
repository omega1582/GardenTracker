namespace GardenTracker.Core.Entities;

public class PlantType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<PlantVariety> Varieties { get; set; } = [];
    public ICollection<MarketPrice> MarketPrices { get; set; } = [];
}
