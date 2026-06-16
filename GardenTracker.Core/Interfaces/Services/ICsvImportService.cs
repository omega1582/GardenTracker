using GardenTracker.Core.Models;

namespace GardenTracker.Core.Interfaces.Services;

public interface ICsvImportService
{
    Task<CsvImportResult> ImportInventoryAsync(int userId, Stream csvStream);
}
