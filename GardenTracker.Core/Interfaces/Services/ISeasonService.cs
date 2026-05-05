using GardenTracker.Core.Entities;

namespace GardenTracker.Core.Interfaces.Services;

public interface ISeasonService
{
    Task<IEnumerable<Season>> GetByGardenAsync(int gardenId, int userId);
    Task<Season?> GetByYearAsync(int gardenId, int year, int userId);
    Task<Season> CreateAsync(int gardenId, int userId, int year, string? notes);
    Task<bool> UpdateAsync(int gardenId, int year, int userId, string? notes);
}
