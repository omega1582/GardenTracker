namespace GardenTracker.Api.DTOs.Responses.Reports;

public class BedBreakdownResponse
{
    public int BedId { get; set; }
    public string BedName { get; set; } = string.Empty;
    public decimal TotalExpenses { get; set; }
    public IEnumerable<HarvestLineResponse> HarvestLines { get; set; } = [];
    public decimal TotalHarvestValue { get; set; }
    public decimal NetCost { get; set; }
}
