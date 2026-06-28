using GardenTracker.Core.Entities;
using GardenTracker.Core.Enums;

namespace GardenTracker.Core.Interfaces.Services;

public interface IPlantVarietyService
{
    Task<IEnumerable<PlantVariety>> GetAllAsync();
    Task<IEnumerable<PlantVariety>> GetByPlantTypeAsync(int plantTypeId);
    Task<PlantVariety?> GetByIdAsync(int id);
    Task<PlantVariety> CreateAsync(int plantTypeId, string name, string? notes, GrowthHabit? growthHabit, int? daysToMaturity, int? spacingInches, SunPreference? sunPreference, bool? isPerennial, string? imageUrl);
    Task<bool> UpdateAsync(int id, string name, string? notes, GrowthHabit? growthHabit, int? daysToMaturity, int? spacingInches, SunPreference? sunPreference, bool? isPerennial, string? imageUrl);
}
