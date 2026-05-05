using GardenTracker.Core.Entities;

namespace GardenTracker.Core.Interfaces.Repositories;

public interface IGardenRepository
{
    Task<IEnumerable<Garden>> GetByUserAsync(int userId);
    Task<Garden?> GetByIdAsync(int id);
    Task<int> CreateAsync(Garden garden);
    Task UpdateAsync(Garden garden);
    Task DeleteAsync(int id);
}
