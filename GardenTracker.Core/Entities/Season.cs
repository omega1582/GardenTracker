namespace GardenTracker.Core.Entities;

public class Season
{
    public int Id { get; set; }
    public int GardenId { get; set; }
    public int Year { get; set; }
    public string? Notes { get; set; }

    public Garden Garden { get; set; } = null!;
    public ICollection<BedPlanting> Plantings { get; set; } = [];
    public ICollection<Harvest> Harvests { get; set; } = [];
    public ICollection<Expense> Expenses { get; set; } = [];
    public ICollection<MarketPrice> MarketPrices { get; set; } = [];
}
