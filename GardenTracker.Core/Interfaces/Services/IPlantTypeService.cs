using GardenTracker.Core.Entities;

namespace GardenTracker.Core.Interfaces.Services;

public interface IPlantTypeService
{
    Task<IEnumerable<PlantType>> GetAllAsync();
    Task<PlantType?> GetByIdAsync(int id);
    Task<PlantType> CreateAsync(string name);
    Task<bool> UpdateAsync(int id, string name);
}
