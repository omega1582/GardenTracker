using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Interfaces.Repositories;
using GardenTracker.Core.Interfaces.Services;
using GardenTracker.Core.Models;

namespace GardenTracker.Services;

public class WaterBillCsvImportService(IWaterBillRepository waterBillRepository) : IWaterBillCsvImportService
{
    public async Task<CsvImportResult> ImportAsync(int userId, Stream csvStream)
    {
        var result = new CsvImportResult();

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            TrimOptions = TrimOptions.Trim,
            MissingFieldFound = null,
        };

        using var reader = new StreamReader(csvStream);
        using var csv = new CsvReader(reader, config);

        List<WaterBillCsvRow> rows;
        try
        {
            rows = csv.GetRecords<WaterBillCsvRow>().ToList();
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Failed to parse CSV: {ex.Message}");
            return result;
        }

        foreach (var row in rows)
        {
            try
            {
                if (row.Year < 2000 || row.Year > 2100)
                {
                    result.Errors.Add($"Row {row.Year}/{row.Month}: invalid year '{row.Year}'.");
                    continue;
                }

                if (row.Month < 1 || row.Month > 12)
                {
                    result.Errors.Add($"Row {row.Year}/{row.Month}: invalid month '{row.Month}'.");
                    continue;
                }

                var existing = await waterBillRepository.GetByYearMonthAsync(userId, row.Year, row.Month);

                if (existing != null)
                {
                    existing.UsageCcf = row.UsageCcf;
                    existing.UsageGallons = row.UsageGallons;
                    existing.TotalCost = row.TotalCost;
                    existing.IsGardenActive = row.IsGardenActive;
                    existing.Notes = string.IsNullOrWhiteSpace(row.Notes) ? existing.Notes : row.Notes;
                    await waterBillRepository.UpdateAsync(existing);
                    result.Updated++;
                }
                else
                {
                    var bill = new WaterBill
                    {
                        UserId = userId,
                        Year = row.Year,
                        Month = row.Month,
                        UsageCcf = row.UsageCcf,
                        UsageGallons = row.UsageGallons,
                        TotalCost = row.TotalCost,
                        IsGardenActive = row.IsGardenActive,
                        Notes = row.Notes,
                    };
                    await waterBillRepository.CreateAsync(bill);
                    result.Created++;
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Row {row.Year}/{row.Month}: {ex.Message}");
            }
        }

        return result;
    }
}
