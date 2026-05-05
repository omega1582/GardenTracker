using GardenTracker.Core.Entities;
using GardenTracker.Core.Interfaces.Repositories;
using GardenTracker.Core.Interfaces.Services;

namespace GardenTracker.Services;

public class SeasonService(ISeasonRepository seasonRepository, IGardenRepository gardenRepository) : ISeasonService
{
    public async Task<IEnumerable<Season>> GetByGardenAsync(int gardenId, int userId)
    {
        var garden = await gardenRepository.GetByIdAsync(gardenId);
        if (garden?.UserId != userId) return [];
        return await seasonRepository.GetByGardenAsync(gardenId);
    }

    public async Task<Season?> GetByYearAsync(int gardenId, int year, int userId)
    {
        var garden = await gardenRepository.GetByIdAsync(gardenId);
        if (garden?.UserId != userId) return null;
        return await seasonRepository.GetByYearAsync(gardenId, year);
    }

    public async Task<Season> CreateAsync(int gardenId, int userId, int year, string? notes)
    {
        var season = new Season { GardenId = gardenId, Year = year, Notes = notes };
        season.Id = await seasonRepository.CreateAsync(season);
        return season;
    }

    public async Task<bool> UpdateAsync(int gardenId, int year, int userId, string? notes)
    {
        var garden = await gardenRepository.GetByIdAsync(gardenId);
        if (garden?.UserId != userId) return false;
        var season = await seasonRepository.GetByYearAsync(gardenId, year);
        if (season == null) return false;
        season.Notes = notes;
        await seasonRepository.UpdateAsync(season);
        return true;
    }
}
