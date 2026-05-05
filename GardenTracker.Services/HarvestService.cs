using GardenTracker.Core.Entities;
using GardenTracker.Core.Enums;
using GardenTracker.Core.Interfaces.Repositories;
using GardenTracker.Core.Interfaces.Services;

namespace GardenTracker.Services;

public class HarvestService(IHarvestRepository harvestRepository, ISeasonRepository seasonRepository, IGardenRepository gardenRepository) : IHarvestService
{
    public async Task<IEnumerable<Harvest>> GetBySeasonAsync(int gardenId, int year, int userId, int? bedId, int? plantVarietyId)
    {
        var garden = await gardenRepository.GetByIdAsync(gardenId);
        if (garden?.UserId != userId) return [];
        return await harvestRepository.GetBySeasonAsync(gardenId, year, bedId, plantVarietyId);
    }

    public async Task<Harvest?> GetByIdAsync(int id, int userId)
    {
        var harvest = await harvestRepository.GetByIdAsync(id);
        if (harvest == null) return null;
        var season = await seasonRepository.GetByIdAsync(harvest.SeasonId);
        var garden = await gardenRepository.GetByIdAsync(season!.GardenId);
        return garden?.UserId == userId ? harvest : null;
    }

    public async Task<Harvest> CreateAsync(int gardenId, int year, int userId, Harvest harvest)
    {
        var season = await seasonRepository.GetByYearAsync(gardenId, year);
        harvest.SeasonId = season!.Id;
        harvest.Id = await harvestRepository.CreateAsync(harvest);
        return harvest;
    }

    public async Task<bool> UpdateAsync(int id, int userId, decimal quantity, HarvestUnit unit, DateOnly harvestDate, string? notes)
    {
        var harvest = await GetByIdAsync(id, userId);
        if (harvest == null) return false;
        harvest.Quantity = quantity; harvest.Unit = unit; harvest.HarvestDate = harvestDate; harvest.Notes = notes;
        await harvestRepository.UpdateAsync(harvest);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        var harvest = await GetByIdAsync(id, userId);
        if (harvest == null) return false;
        await harvestRepository.DeleteAsync(id);
        return true;
    }
}
