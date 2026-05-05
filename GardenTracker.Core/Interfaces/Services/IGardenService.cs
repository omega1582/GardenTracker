using GardenTracker.Core.Entities;

namespace GardenTracker.Core.Interfaces.Services;

public interface IGardenService
{
    Task<IEnumerable<Garden>> GetByUserAsync(int userId);
    Task<Garden?> GetByIdAsync(int id, int userId);
    Task<Garden> CreateAsync(int userId, string name, string? location, string? notes);
    Task<bool> UpdateAsync(int id, int userId, string name, string? location, string? notes);
    Task<bool> DeleteAsync(int id, int userId);
}
