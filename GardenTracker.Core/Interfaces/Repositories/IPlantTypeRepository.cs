using GardenTracker.Core.Entities;

namespace GardenTracker.Core.Interfaces.Repositories;

public interface IPlantTypeRepository
{
    Task<IEnumerable<PlantType>> GetAllAsync();
    Task<PlantType?> GetByIdAsync(int id);
    Task<PlantType?> GetByNameAsync(string name);
    Task<int> CreateAsync(PlantType plantType);
    Task UpdateAsync(PlantType plantType);
}
