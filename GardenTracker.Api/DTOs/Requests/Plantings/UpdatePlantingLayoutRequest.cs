namespace GardenTracker.Api.DTOs.Requests.Plantings;

public class UpdatePlantingLayoutRequest
{
    public decimal? PositionX { get; set; }
    public decimal? PositionY { get; set; }
    public decimal? LayoutWidth { get; set; }
    public decimal? LayoutHeight { get; set; }
}
