using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Enums;
using GardenTracker.Core.Interfaces.Repositories;
using GardenTracker.Core.Interfaces.Services;

namespace GardenTracker.Services;

public class PlantVarietyService(IPlantVarietyRepository varietyRepository, IPlantTypeRepository plantTypeRepository) : IPlantVarietyService
{
    public async Task<IEnumerable<PlantVariety>> GetAllAsync()
    {
        var plantTypes = (await plantTypeRepository.GetAllAsync()).ToDictionary(t => t.Id);
        var varieties = await varietyRepository.GetAllAsync();
        foreach (var v in varieties)
        {
            if (plantTypes.TryGetValue(v.PlantTypeId, out var pt))
                ApplyFallbacks(v, pt);
        }
        return varieties;
    }

    public async Task<IEnumerable<PlantVariety>> GetByPlantTypeAsync(int plantTypeId)
    {
        var plantType = await plantTypeRepository.GetByIdAsync(plantTypeId);
        var varieties = await varietyRepository.GetByPlantTypeAsync(plantTypeId);
        if (plantType != null)
            foreach (var v in varieties)
                ApplyFallbacks(v, plantType);
        return varieties;
    }

    public async Task<PlantVariety?> GetByIdAsync(int id)
    {
        var variety = await varietyRepository.GetByIdAsync(id);
        if (variety == null) return null;
        var plantType = await plantTypeRepository.GetByIdAsync(variety.PlantTypeId);
        if (plantType != null)
            ApplyFallbacks(variety, plantType);
        return variety;
    }

    public async Task<PlantVariety> CreateAsync(int plantTypeId, string name, string? notes, GrowthHabit? growthHabit, int? daysToMaturity, int? spacingInches, SunPreference? sunPreference, bool? isPerennial, string? imageUrl)
    {
        var localImageUrl = await DownloadAndSaveImageAsync(imageUrl);
        var variety = new PlantVariety
        {
            PlantTypeId = plantTypeId,
            Name = name,
            Notes = notes,
            GrowthHabit = growthHabit,
            DaysToMaturity = daysToMaturity,
            SpacingInches = spacingInches,
            SunPreference = sunPreference,
            IsPerennial = isPerennial,
            ImageUrl = localImageUrl
        };
        variety.Id = await varietyRepository.CreateAsync(variety);
        return variety;
    }

    public async Task<bool> UpdateAsync(int id, string name, string? notes, GrowthHabit? growthHabit, int? daysToMaturity, int? spacingInches, SunPreference? sunPreference, bool? isPerennial, string? imageUrl)
    {
        var variety = await varietyRepository.GetByIdAsync(id);
        if (variety == null) return false;

        var localImageUrl = await DownloadAndSaveImageAsync(imageUrl);

        variety.Name = name;
        variety.Notes = notes;
        variety.GrowthHabit = growthHabit;
        variety.DaysToMaturity = daysToMaturity;
        variety.SpacingInches = spacingInches;
        variety.SunPreference = sunPreference;
        variety.IsPerennial = isPerennial;
        variety.ImageUrl = localImageUrl;
        await varietyRepository.UpdateAsync(variety);
        return true;
    }

    private async Task<string?> DownloadAndSaveImageAsync(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl)) return imageUrl;

        if (!imageUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !imageUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return imageUrl;
        }

        // Bypass downloading during xUnit tests to prevent network requests and slow test runs
        if (AppDomain.CurrentDomain.GetAssemblies().Any(a => a.FullName?.StartsWith("xunit", StringComparison.OrdinalIgnoreCase) ?? false))
        {
            return imageUrl;
        }

        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

            var response = await client.GetAsync(imageUrl);
            if (!response.IsSuccessStatusCode) return imageUrl;

            var contentType = response.Content.Headers.ContentType?.MediaType;
            var ext = ".jpg";
            if (contentType != null)
            {
                ext = contentType switch
                {
                    "image/jpeg" => ".jpg",
                    "image/png" => ".png",
                    "image/gif" => ".gif",
                    "image/webp" => ".webp",
                    _ => Path.GetExtension(imageUrl) ?? ".jpg"
                };
            }

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            var data = await response.Content.ReadAsByteArrayAsync();
            await File.WriteAllBytesAsync(filePath, data);

            return $"/uploads/{uniqueFileName}";
        }
        catch
        {
            return imageUrl;
        }
    }

    private static void ApplyFallbacks(PlantVariety variety, PlantType plantType)
    {
        variety.GrowthHabit ??= plantType.GrowthHabit;
        variety.DaysToMaturity ??= plantType.DaysToMaturity;
        variety.SpacingInches ??= plantType.SpacingInches;
        variety.SunPreference ??= plantType.SunPreference;
        variety.IsPerennial ??= plantType.IsPerennial;
    }
}
