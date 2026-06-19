using GardenTracker.Core.Models;

namespace GardenTracker.Core.Interfaces.Services;

public interface IWaterBillCsvImportService
{
    Task<CsvImportResult> ImportAsync(int userId, Stream csvStream);
}
