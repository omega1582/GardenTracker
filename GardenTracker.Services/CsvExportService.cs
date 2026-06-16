using System.Globalization;
using CsvHelper;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Interfaces.Repositories;
using GardenTracker.Core.Interfaces.Services;

namespace GardenTracker.Services;

public class CsvExportService(IInventoryRepository inventoryRepository) : ICsvExportService
{
    public async Task<byte[]> ExportInventoryAsync(int userId)
    {
        var items = await inventoryRepository.GetByUserAsync(userId);

        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms, leaveOpen: true);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        csv.WriteHeader<InventoryCsvRow>();
        await csv.NextRecordAsync();

        foreach (var item in items)
        {
            csv.WriteRecord(new InventoryCsvRow
            {
                PlantTypeName = item.PlantTypeName ?? string.Empty,
                PlantVarietyName = item.PlantVarietyName ?? string.Empty,
                Type = item.Type.ToString(),
                QuantityPurchased = item.QuantityPurchased,
                QuantityRemaining = item.QuantityRemaining,
                TotalCost = item.TotalCost,
                PurchaseDate = item.PurchaseDate.ToString("yyyy-MM-dd"),
                SupplierName = item.SupplierName,
                SupplierType = null,
                Notes = item.Notes
            });
            await csv.NextRecordAsync();
        }

        await writer.FlushAsync();
        return ms.ToArray();
    }
}
