using GardenTracker.Core.Entities;
using GardenTracker.Core.Enums;
using GardenTracker.Core.Interfaces.Repositories;
using GardenTracker.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace GardenTracker.Services;

public class HarvestService(IHarvestRepository harvestRepository, ISeasonRepository seasonRepository, IGardenRepository gardenRepository, ILogger<HarvestService> logger) : IHarvestService
{
    public async Task<IEnumerable<Harvest>> GetBySeasonAsync(int gardenId, int year, int userId, int? bedId, int? plantVarietyId)
    {
        var garden = await gardenRepository.GetByIdAsync(gardenId);
        if (garden?.UserId != userId)
        {
            logger.LogInformation("Harvests request for garden {GardenId} year {Year} denied — not owned by user {UserId}", gardenId, year, userId);
            return [];
        }
        return await harvestRepository.GetBySeasonAsync(gardenId, year, bedId, plantVarietyId);
    }

    public async Task<Harvest?> GetByIdAsync(int id, int userId)
    {
        var harvest = await harvestRepository.GetByIdAsync(id);
        if (harvest == null) return null;
        var season = await seasonRepository.GetByIdAsync(harvest.SeasonId);
        var garden = await gardenRepository.GetByIdAsync(season!.GardenId);
        if (garden?.UserId != userId)
        {
            logger.LogInformation("Harvest {HarvestId} access denied — not owned by user {UserId}", id, userId);
            return null;
        }
        return harvest;
    }

    public async Task<Harvest> CreateAsync(int gardenId, int year, int userId, Harvest harvest)
    {
        var season = await seasonRepository.GetByYearAsync(gardenId, year);
        harvest.SeasonId = season!.Id;
        harvest.Id = await harvestRepository.CreateAsync(harvest);
        logger.LogInformation("Harvest {HarvestId} created in garden {GardenId} year {Year} by user {UserId}", harvest.Id, gardenId, year, userId);
        return harvest;
    }

    public async Task<bool> UpdateAsync(int id, int userId, decimal quantity, HarvestUnit unit, DateOnly harvestDate, string? notes)
    {
        var harvest = await GetByIdAsync(id, userId);
        if (harvest == null)
        {
            logger.LogInformation("Harvest {HarvestId} update failed — not found or not owned by user {UserId}", id, userId);
            return false;
        }
        harvest.Quantity = quantity; harvest.Unit = unit; harvest.HarvestDate = harvestDate; harvest.Notes = notes;
        await harvestRepository.UpdateAsync(harvest);
        logger.LogInformation("Harvest {HarvestId} updated by user {UserId}", id, userId);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        var harvest = await GetByIdAsync(id, userId);
        if (harvest == null)
        {
            logger.LogInformation("Harvest {HarvestId} delete failed — not found or not owned by user {UserId}", id, userId);
            return false;
        }
        await harvestRepository.DeleteAsync(id);
        logger.LogInformation("Harvest {HarvestId} deleted by user {UserId}", id, userId);
        return true;
    }
}
