using GardenTracker.Core.Entities;

namespace GardenTracker.Core.Interfaces.Services;

public interface IBedService
{
    Task<IEnumerable<Bed>> GetByGardenAsync(int gardenId, int userId);
    Task<Bed?> GetByIdAsync(int id, int userId);
    Task<Bed> CreateAsync(int gardenId, int userId, Bed bed);
    Task<bool> UpdateAsync(int id, int userId, string name, string? material, int expectedLifespanYears, string? notes);
    Task<bool> UpdatePositionAsync(int id, int userId, decimal? positionX, decimal? positionY);
}
