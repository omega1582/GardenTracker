using GardenTracker.Core.Entities;
using GardenTracker.Core.Enums;
using GardenTracker.Core.Interfaces.Repositories;
using GardenTracker.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace GardenTracker.Services;

public class PlantingService(IPlantingRepository plantingRepository, ISeasonRepository seasonRepository, IGardenRepository gardenRepository, ILogger<PlantingService> logger) : IPlantingService
{
    public async Task<IEnumerable<BedPlanting>> GetBySeasonAsync(int gardenId, int year, int userId, int? bedId, int? plantTypeId)
    {
        var garden = await gardenRepository.GetByIdAsync(gardenId);
        if (garden?.UserId != userId)
        {
            logger.LogInformation("Plantings request for garden {GardenId} year {Year} denied — not owned by user {UserId}", gardenId, year, userId);
            return [];
        }
        return await plantingRepository.GetBySeasonAsync(gardenId, year, bedId, plantTypeId);
    }

    public async Task<BedPlanting?> GetByIdAsync(int id, int userId)
    {
        var planting = await plantingRepository.GetByIdAsync(id);
        if (planting == null) return null;
        var season = await seasonRepository.GetByIdAsync(planting.SeasonId);
        var garden = await gardenRepository.GetByIdAsync(season!.GardenId);
        if (garden?.UserId != userId)
        {
            logger.LogInformation("Planting {PlantingId} access denied — not owned by user {UserId}", id, userId);
            return null;
        }
        return planting;
    }

    public async Task<BedPlanting> CreateAsync(int gardenId, int year, int userId, BedPlanting planting)
    {
        var season = await seasonRepository.GetByYearAsync(gardenId, year);
        planting.SeasonId = season!.Id;
        planting.Id = await plantingRepository.CreateAsync(planting);
        logger.LogInformation("Planting {PlantingId} created in garden {GardenId} year {Year} by user {UserId}", planting.Id, gardenId, year, userId);
        return planting;
    }

    public async Task<bool> UpdateAsync(int id, int userId, int? supplierId, StartMethod startMethod, int quantity, decimal totalCost, int? sourceHarvestId, string? notes)
    {
        var planting = await GetByIdAsync(id, userId);
        if (planting == null)
        {
            logger.LogInformation("Planting {PlantingId} update failed — not found or not owned by user {UserId}", id, userId);
            return false;
        }
        planting.SupplierId = supplierId; planting.StartMethod = startMethod;
        planting.Quantity = quantity; planting.TotalCost = totalCost;
        planting.SourceHarvestId = sourceHarvestId; planting.Notes = notes;
        await plantingRepository.UpdateAsync(planting);
        logger.LogInformation("Planting {PlantingId} updated by user {UserId}", id, userId);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        var planting = await GetByIdAsync(id, userId);
        if (planting == null)
        {
            logger.LogInformation("Planting {PlantingId} delete failed — not found or not owned by user {UserId}", id, userId);
            return false;
        }
        await plantingRepository.DeleteAsync(id);
        logger.LogInformation("Planting {PlantingId} deleted by user {UserId}", id, userId);
        return true;
    }
}
