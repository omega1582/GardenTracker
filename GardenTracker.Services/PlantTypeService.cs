using GardenTracker.Core.Entities;
using GardenTracker.Core.Interfaces.Repositories;
using GardenTracker.Core.Interfaces.Services;

namespace GardenTracker.Services;

public class PlantTypeService(IPlantTypeRepository plantTypeRepository) : IPlantTypeService
{
    public Task<IEnumerable<PlantType>> GetAllAsync() => plantTypeRepository.GetAllAsync();

    public Task<PlantType?> GetByIdAsync(int id) => plantTypeRepository.GetByIdAsync(id);

    public async Task<PlantType> CreateAsync(string name)
    {
        var plantType = new PlantType { Name = name };
        plantType.Id = await plantTypeRepository.CreateAsync(plantType);
        return plantType;
    }

    public async Task<bool> UpdateAsync(int id, string name)
    {
        var plantType = await plantTypeRepository.GetByIdAsync(id);
        if (plantType == null) return false;
        plantType.Name = name;
        await plantTypeRepository.UpdateAsync(plantType);
        return true;
    }
}
