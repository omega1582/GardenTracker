namespace GardenTracker.Api.DTOs.Responses.Gardens;

public class GardenResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
