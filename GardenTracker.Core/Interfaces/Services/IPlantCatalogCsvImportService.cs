using GardenTracker.Core.Models;

namespace GardenTracker.Core.Interfaces.Services;

public interface IPlantCatalogCsvImportService
{
    Task<CsvImportResult> ImportAsync(Stream csvStream);
}
