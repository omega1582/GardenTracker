using GardenTracker.Core.Entities;
using GardenTracker.Core.Interfaces.Repositories;
using GardenTracker.Core.Interfaces.Services;

namespace GardenTracker.Services;

public class PlantVarietyService(IPlantVarietyRepository varietyRepository) : IPlantVarietyService
{
    public Task<IEnumerable<PlantVariety>> GetByPlantTypeAsync(int plantTypeId) =>
        varietyRepository.GetByPlantTypeAsync(plantTypeId);

    public Task<PlantVariety?> GetByIdAsync(int id) => varietyRepository.GetByIdAsync(id);

    public async Task<PlantVariety> CreateAsync(int plantTypeId, string name, string? notes)
    {
        var variety = new PlantVariety { PlantTypeId = plantTypeId, Name = name, Notes = notes };
        variety.Id = await varietyRepository.CreateAsync(variety);
        return variety;
    }

    public async Task<bool> UpdateAsync(int id, string name, string? notes)
    {
        var variety = await varietyRepository.GetByIdAsync(id);
        if (variety == null) return false;
        variety.Name = name; variety.Notes = notes;
        await varietyRepository.UpdateAsync(variety);
        return true;
    }
}
