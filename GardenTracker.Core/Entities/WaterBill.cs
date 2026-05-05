namespace GardenTracker.Core.Entities;

public class WaterBill
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal UsageCcf { get; set; }
    public decimal UsageGallons { get; set; }
    public decimal TotalCost { get; set; }
    public bool IsGardenActive { get; set; }
    public string? Notes { get; set; }

    public User User { get; set; } = null!;
}
