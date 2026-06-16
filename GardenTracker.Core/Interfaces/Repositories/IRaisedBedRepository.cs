using GardenTracker.Core.Entities;

namespace GardenTracker.Core.Interfaces.Repositories;

public interface IBedRepository
{
    Task<IEnumerable<Bed>> GetByGardenAsync(int gardenId);
    Task<Bed?> GetByIdAsync(int id);
    Task<int> CreateAsync(Bed bed);
    Task UpdateAsync(Bed bed);
    Task UpdatePositionAsync(int id, decimal? positionX, decimal? positionY);
}
