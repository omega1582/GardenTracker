namespace GardenTracker.Api.DTOs.Responses.Seasons;

public class SeasonResponse
{
    public int Id { get; set; }
    public int GardenId { get; set; }
    public int Year { get; set; }
    public string? Notes { get; set; }
}
