using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Enums;
using GardenTracker.Core.Interfaces.Repositories;
using GardenTracker.Core.Interfaces.Services;
using GardenTracker.Core.Models;

namespace GardenTracker.Services;

public class PlantCatalogCsvImportService(
    IPlantTypeRepository plantTypeRepository,
    IPlantVarietyRepository plantVarietyRepository) : IPlantCatalogCsvImportService
{
    public async Task<CsvImportResult> ImportAsync(Stream csvStream)
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

        List<PlantCatalogCsvRow> rows;
        try
        {
            rows = csv.GetRecords<PlantCatalogCsvRow>().ToList();
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Failed to parse CSV: {ex.Message}");
            return result;
        }

        // Cache types found/created this import to avoid redundant DB calls
        var typeCache = new Dictionary<string, PlantType>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(row.PlantTypeName))
                {
                    result.Errors.Add($"Row skipped: PlantTypeName is required.");
                    continue;
                }

                var growthHabit = ParseEnum<GrowthHabit>(row.GrowthHabit);
                var sunPreference = ParseEnum<SunPreference>(row.SunPreference);

                // Find or create the plant type
                if (!typeCache.TryGetValue(row.PlantTypeName, out var plantType))
                {
                    plantType = await plantTypeRepository.GetByNameAsync(row.PlantTypeName);
                    if (plantType == null)
                    {
                        plantType = new PlantType
                        {
                            Name = row.PlantTypeName,
                            GrowthHabit = growthHabit,
                            DaysToMaturity = row.DaysToMaturity,
                            SpacingInches = row.SpacingInches,
                            SunPreference = sunPreference,
                            IsPerennial = row.IsPerennial,
                        };
                        plantType.Id = await plantTypeRepository.CreateAsync(plantType);
                        result.Created++;
                    }
                    else if (string.IsNullOrWhiteSpace(row.PlantVarietyName))
                    {
                        // Row is a type-only row — update the type's attributes
                        plantType.GrowthHabit = growthHabit ?? plantType.GrowthHabit;
                        plantType.DaysToMaturity = row.DaysToMaturity ?? plantType.DaysToMaturity;
                        plantType.SpacingInches = row.SpacingInches ?? plantType.SpacingInches;
                        plantType.SunPreference = sunPreference ?? plantType.SunPreference;
                        plantType.IsPerennial = row.IsPerennial ?? plantType.IsPerennial;
                        await plantTypeRepository.UpdateAsync(plantType);
                        result.Updated++;
                    }
                    typeCache[row.PlantTypeName] = plantType;
                }

                // If no variety name, this row was just for the type
                if (string.IsNullOrWhiteSpace(row.PlantVarietyName))
                    continue;

                // Find or create the variety
                var existing = await plantVarietyRepository.GetByPlantTypeAndNameAsync(plantType.Id, row.PlantVarietyName);
                if (existing != null)
                {
                    existing.GrowthHabit = growthHabit ?? existing.GrowthHabit;
                    existing.DaysToMaturity = row.DaysToMaturity ?? existing.DaysToMaturity;
                    existing.SpacingInches = row.SpacingInches ?? existing.SpacingInches;
                    existing.SunPreference = sunPreference ?? existing.SunPreference;
                    existing.IsPerennial = row.IsPerennial ?? existing.IsPerennial;
                    if (!string.IsNullOrWhiteSpace(row.Notes)) existing.Notes = row.Notes;
                    await plantVarietyRepository.UpdateAsync(existing);
                    result.Updated++;
                }
                else
                {
                    var variety = new PlantVariety
                    {
                        PlantTypeId = plantType.Id,
                        Name = row.PlantVarietyName,
                        GrowthHabit = growthHabit,
                        DaysToMaturity = row.DaysToMaturity,
                        SpacingInches = row.SpacingInches,
                        SunPreference = sunPreference,
                        IsPerennial = row.IsPerennial,
                        Notes = row.Notes,
                    };
                    await plantVarietyRepository.CreateAsync(variety);
                    result.Created++;
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Row '{row.PlantTypeName}/{row.PlantVarietyName}': {ex.Message}");
            }
        }

        return result;
    }

    private static T? ParseEnum<T>(string? value) where T : struct, Enum =>
        Enum.TryParse<T>(value, ignoreCase: true, out var parsed) ? parsed : null;
}
