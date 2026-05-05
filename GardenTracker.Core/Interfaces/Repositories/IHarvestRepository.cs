using GardenTracker.Core.Entities;

namespace GardenTracker.Core.Interfaces.Repositories;

public interface IHarvestRepository
{
    Task<IEnumerable<Harvest>> GetBySeasonAsync(int gardenId, int year, int? bedId = null, int? plantVarietyId = null);
    Task<Harvest?> GetByIdAsync(int id);
    Task<int> CreateAsync(Harvest harvest);
    Task UpdateAsync(Harvest harvest);
    Task DeleteAsync(int id);
}
