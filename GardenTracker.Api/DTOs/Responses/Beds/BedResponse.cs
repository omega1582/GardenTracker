namespace GardenTracker.Api.DTOs.Responses.Beds;

public class BedResponse
{
    public int Id { get; set; }
    public int GardenId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal LengthFt { get; set; }
    public decimal WidthFt { get; set; }
    public decimal DepthIn { get; set; }
    public string? Material { get; set; }
    public int ExpectedLifespanYears { get; set; }
    public DateOnly InstalledDate { get; set; }
    public string? Notes { get; set; }
}
