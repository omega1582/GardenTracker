using GardenTracker.Core.Entities;

namespace GardenTracker.Core.Interfaces.Repositories;

public interface IPlantingRepository
{
    Task<IEnumerable<BedPlanting>> GetBySeasonAsync(int gardenId, int year, int? bedId = null, int? plantTypeId = null);
    Task<BedPlanting?> GetByIdAsync(int id);
    Task<int> CreateAsync(BedPlanting planting);
    Task UpdateAsync(BedPlanting planting);
    Task DeleteAsync(int id);
}
