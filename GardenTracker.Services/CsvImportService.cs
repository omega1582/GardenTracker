using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Enums;
using GardenTracker.Core.Interfaces.Repositories;
using GardenTracker.Core.Interfaces.Services;
using GardenTracker.Core.Models;

namespace GardenTracker.Services;

public class CsvImportService(
    IInventoryRepository inventoryRepository,
    IPlantTypeRepository plantTypeRepository,
    IPlantVarietyRepository plantVarietyRepository,
    ISupplierRepository supplierRepository) : ICsvImportService
{
    public async Task<CsvImportResult> ImportInventoryAsync(int userId, Stream csvStream)
    {
        var result = new CsvImportResult();

        List<InventoryCsvRow> rows;
        try
        {
            rows = ReadCsvRows(csvStream);
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Failed to parse CSV: {ex.Message}");
            return result;
        }

        // Load all existing user inventory items once for upsert matching
        var existingItems = (await inventoryRepository.GetByUserAsync(userId)).ToList();

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            int rowNum = i + 2; // 1-based, row 1 is header
            try
            {
                await ProcessRowAsync(userId, row, existingItems, result);
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Row {rowNum} ({row.PlantTypeName}/{row.PlantVarietyName}): {ex.Message}");
            }
        }

        return result;
    }

    private async Task ProcessRowAsync(int userId, InventoryCsvRow row, List<InventoryItem> existingItems, CsvImportResult result)
    {
        if (string.IsNullOrWhiteSpace(row.PlantTypeName))
            throw new InvalidOperationException("PlantTypeName is required.");
        if (string.IsNullOrWhiteSpace(row.PlantVarietyName))
            throw new InvalidOperationException("PlantVarietyName is required.");
        if (!Enum.TryParse<InventoryType>(row.Type, ignoreCase: true, out var inventoryType))
            throw new InvalidOperationException($"Invalid Type '{row.Type}'. Must be Seed or Plant.");
        if (!DateOnly.TryParseExact(row.PurchaseDate, "yyyy-MM-dd", out var purchaseDate))
            throw new InvalidOperationException($"Invalid PurchaseDate '{row.PurchaseDate}'. Expected yyyy-MM-dd.");

        // Find or create PlantType
        var plantType = await plantTypeRepository.GetByNameAsync(row.PlantTypeName)
            ?? await CreatePlantTypeAsync(row.PlantTypeName);

        // Find or create PlantVariety
        var variety = await plantVarietyRepository.GetByPlantTypeAndNameAsync(plantType.Id, row.PlantVarietyName)
            ?? await CreatePlantVarietyAsync(plantType.Id, row.PlantVarietyName);

        // Find or create Supplier
        int? supplierId = null;
        if (!string.IsNullOrWhiteSpace(row.SupplierName))
        {
            var supplier = await supplierRepository.GetByNameAsync(row.SupplierName)
                ?? await CreateSupplierAsync(row.SupplierName, row.SupplierType);
            supplierId = supplier.Id;
        }

        // Upsert inventory item: match by UserId + PlantVarietyId + PurchaseDate + Type
        var existing = existingItems.FirstOrDefault(x =>
            x.UserId == userId &&
            x.PlantVarietyId == variety.Id &&
            x.PurchaseDate == purchaseDate &&
            x.Type == inventoryType);

        if (existing != null)
        {
            existing.SupplierId = supplierId;
            existing.QuantityPurchased = row.QuantityPurchased;
            existing.QuantityRemaining = row.QuantityRemaining;
            existing.TotalCost = row.TotalCost;
            existing.Notes = row.Notes;
            await inventoryRepository.UpdateAsync(existing);
            await inventoryRepository.UpdateRemainingQuantityAsync(existing.Id, row.QuantityRemaining);
            result.Updated++;
        }
        else
        {
            var newItem = new InventoryItem
            {
                UserId = userId,
                PlantVarietyId = variety.Id,
                SupplierId = supplierId,
                Type = inventoryType,
                QuantityPurchased = row.QuantityPurchased,
                QuantityRemaining = row.QuantityRemaining,
                TotalCost = row.TotalCost,
                PurchaseDate = purchaseDate,
                Notes = row.Notes
            };
            newItem.Id = await inventoryRepository.CreateAsync(newItem);
            existingItems.Add(newItem); // prevent duplicate create within same import
            result.Created++;
        }
    }

    private async Task<PlantType> CreatePlantTypeAsync(string name)
    {
        var pt = new PlantType { Name = name };
        pt.Id = await plantTypeRepository.CreateAsync(pt);
        return pt;
    }

    private async Task<PlantVariety> CreatePlantVarietyAsync(int plantTypeId, string name)
    {
        var v = new PlantVariety { PlantTypeId = plantTypeId, Name = name };
        v.Id = await plantVarietyRepository.CreateAsync(v);
        return v;
    }

    private async Task<Supplier> CreateSupplierAsync(string name, string? supplierTypeStr)
    {
        var supplierType = Enum.TryParse<SupplierType>(supplierTypeStr, ignoreCase: true, out var parsed)
            ? parsed
            : SupplierType.Other;
        var s = new Supplier { Name = name, SupplierType = supplierType };
        s.Id = await supplierRepository.CreateAsync(s);
        return s;
    }

    private static List<InventoryCsvRow> ReadCsvRows(Stream csvStream)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            TrimOptions = TrimOptions.Trim,
            MissingFieldFound = null,
        };
        using var reader = new StreamReader(csvStream, leaveOpen: true);
        using var csv = new CsvReader(reader, config);
        return csv.GetRecords<InventoryCsvRow>().ToList();
    }
}
