namespace GardenTracker.Core.Entities;

public class RaisedBed
{
    public int Id { get; set; }
    public int GardenId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal LengthFt { get; set; }
    public decimal WidthFt { get; set; }
    public decimal DepthIn { get; set; }
    public string? Material { get; set; }
    public int ExpectedLifespanYears { get; set; } = 10;
    public DateOnly InstalledDate { get; set; }
    public DateOnly? RetiredDate { get; set; }
    public string? Notes { get; set; }

    public Garden Garden { get; set; } = null!;
    public ICollection<BedPlanting> Plantings { get; set; } = [];
    public ICollection<Harvest> Harvests { get; set; } = [];
    public ICollection<Expense> Expenses { get; set; } = [];
}
