namespace GardenTracker.Core.Entities;

public class Garden
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
    public ICollection<Bed> Beds { get; set; } = [];
    public ICollection<Season> Seasons { get; set; } = [];
}
