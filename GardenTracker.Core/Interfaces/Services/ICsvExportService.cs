namespace GardenTracker.Core.Interfaces.Services;

public interface ICsvExportService
{
    Task<byte[]> ExportInventoryAsync(int userId);
}
