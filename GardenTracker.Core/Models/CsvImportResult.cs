namespace GardenTracker.Core.Models;

public class CsvImportResult
{
    public int Created { get; set; }
    public int Updated { get; set; }
    public List<string> Errors { get; set; } = [];
}
