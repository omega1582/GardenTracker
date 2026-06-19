using GardenTracker.Core.Entities;

namespace GardenTracker.Core.Interfaces.Services;

public interface IBedService
{
    Task<IEnumerable<Bed>> GetByGardenAsync(int gardenId, int userId);
    Task<Bed?> GetByIdAsync(int id, int userId);
    Task<Bed> CreateAsync(int gardenId, int userId, Bed bed);
    Task<bool> UpdateAsync(int id, int userId, string name, decimal lengthFt, decimal widthFt, decimal depthIn, string? material, int expectedLifespanYears, DateOnly installedDate, string? notes);
    Task<bool> UpdatePositionAsync(int id, int userId, decimal? positionX, decimal? positionY);
}
