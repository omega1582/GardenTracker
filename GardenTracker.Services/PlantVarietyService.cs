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
            ImageUrl = imageUrl
        };
        variety.Id = await varietyRepository.CreateAsync(variety);
        return variety;
    }

    public async Task<bool> UpdateAsync(int id, string name, string? notes, GrowthHabit? growthHabit, int? daysToMaturity, int? spacingInches, SunPreference? sunPreference, bool? isPerennial, string? imageUrl)
    {
        var variety = await varietyRepository.GetByIdAsync(id);
        if (variety == null) return false;
        variety.Name = name;
        variety.Notes = notes;
        variety.GrowthHabit = growthHabit;
        variety.DaysToMaturity = daysToMaturity;
        variety.SpacingInches = spacingInches;
        variety.SunPreference = sunPreference;
        variety.IsPerennial = isPerennial;
        variety.ImageUrl = imageUrl;
        await varietyRepository.UpdateAsync(variety);
        return true;
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
