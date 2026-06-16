using GardenTracker.Core.Entities;
using GardenTracker.Core.Enums;
using GardenTracker.Core.Interfaces.Repositories;
using GardenTracker.Core.Interfaces.Services;

namespace GardenTracker.Services;

public class PlantTypeService(IPlantTypeRepository plantTypeRepository) : IPlantTypeService
{
    public Task<IEnumerable<PlantType>> GetAllAsync() => plantTypeRepository.GetAllAsync();

    public Task<PlantType?> GetByIdAsync(int id) => plantTypeRepository.GetByIdAsync(id);

    public async Task<PlantType> CreateAsync(string name, GrowthHabit? growthHabit, int? daysToMaturity, int? spacingInches, SunPreference? sunPreference, bool? isPerennial)
    {
        var plantType = new PlantType
        {
            Name = name,
            GrowthHabit = growthHabit,
            DaysToMaturity = daysToMaturity,
            SpacingInches = spacingInches,
            SunPreference = sunPreference,
            IsPerennial = isPerennial
        };
        plantType.Id = await plantTypeRepository.CreateAsync(plantType);
        return plantType;
    }

    public async Task<bool> UpdateAsync(int id, string name, GrowthHabit? growthHabit, int? daysToMaturity, int? spacingInches, SunPreference? sunPreference, bool? isPerennial)
    {
        var plantType = await plantTypeRepository.GetByIdAsync(id);
        if (plantType == null) return false;
        plantType.Name = name;
        plantType.GrowthHabit = growthHabit;
        plantType.DaysToMaturity = daysToMaturity;
        plantType.SpacingInches = spacingInches;
        plantType.SunPreference = sunPreference;
        plantType.IsPerennial = isPerennial;
        await plantTypeRepository.UpdateAsync(plantType);
        return true;
    }
}
