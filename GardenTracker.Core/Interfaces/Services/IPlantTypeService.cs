using GardenTracker.Core.Entities;
using GardenTracker.Core.Enums;

namespace GardenTracker.Core.Interfaces.Services;

public interface IPlantTypeService
{
    Task<IEnumerable<PlantType>> GetAllAsync();
    Task<PlantType?> GetByIdAsync(int id);
    Task<PlantType> CreateAsync(string name, PlantCategory category, GrowthHabit? growthHabit, int? daysToMaturity, int? spacingInches, SunPreference? sunPreference, bool? isPerennial);
    Task<bool> UpdateAsync(int id, string name, PlantCategory category, GrowthHabit? growthHabit, int? daysToMaturity, int? spacingInches, SunPreference? sunPreference, bool? isPerennial);
}
