using GardenTracker.Core.Entities;

namespace GardenTracker.Core.Interfaces.Repositories;

public interface IPlantVarietyRepository
{
    Task<IEnumerable<PlantVariety>> GetAllAsync();
    Task<IEnumerable<PlantVariety>> GetByPlantTypeAsync(int plantTypeId);
    Task<PlantVariety?> GetByIdAsync(int id);
    Task<PlantVariety?> GetByPlantTypeAndNameAsync(int plantTypeId, string name);
    Task<int> CreateAsync(PlantVariety variety);
    Task UpdateAsync(PlantVariety variety);
}
