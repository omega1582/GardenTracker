using GardenTracker.Core.Entities;

namespace GardenTracker.Core.Interfaces.Repositories;

public interface ISeasonRepository
{
    Task<IEnumerable<Season>> GetByGardenAsync(int gardenId);
    Task<Season?> GetByIdAsync(int id);
    Task<Season?> GetByYearAsync(int gardenId, int year);
    Task<int> CreateAsync(Season season);
    Task UpdateAsync(Season season);
}
