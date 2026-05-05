using GardenTracker.Core.Entities;

namespace GardenTracker.Core.Interfaces.Services;

public interface IPlantVarietyService
{
    Task<IEnumerable<PlantVariety>> GetByPlantTypeAsync(int plantTypeId);
    Task<PlantVariety?> GetByIdAsync(int id);
    Task<PlantVariety> CreateAsync(int plantTypeId, string name, string? notes);
    Task<bool> UpdateAsync(int id, string name, string? notes);
}
