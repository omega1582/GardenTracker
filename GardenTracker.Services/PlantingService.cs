using GardenTracker.Core.Entities;
using GardenTracker.Core.Enums;
using GardenTracker.Core.Interfaces.Repositories;
using GardenTracker.Core.Interfaces.Services;

namespace GardenTracker.Services;

public class PlantingService(IPlantingRepository plantingRepository, ISeasonRepository seasonRepository, IGardenRepository gardenRepository) : IPlantingService
{
    public async Task<IEnumerable<BedPlanting>> GetBySeasonAsync(int gardenId, int year, int userId, int? bedId, int? plantTypeId)
    {
        var garden = await gardenRepository.GetByIdAsync(gardenId);
        if (garden?.UserId != userId) return [];
        return await plantingRepository.GetBySeasonAsync(gardenId, year, bedId, plantTypeId);
    }

    public async Task<BedPlanting?> GetByIdAsync(int id, int userId)
    {
        var planting = await plantingRepository.GetByIdAsync(id);
        if (planting == null) return null;
        var season = await seasonRepository.GetByIdAsync(planting.SeasonId);
        var garden = await gardenRepository.GetByIdAsync(season!.GardenId);
        return garden?.UserId == userId ? planting : null;
    }

    public async Task<BedPlanting> CreateAsync(int gardenId, int year, int userId, BedPlanting planting)
    {
        var season = await seasonRepository.GetByYearAsync(gardenId, year);
        planting.SeasonId = season!.Id;
        planting.Id = await plantingRepository.CreateAsync(planting);
        return planting;
    }

    public async Task<bool> UpdateAsync(int id, int userId, int? supplierId, StartMethod startMethod, int quantity, decimal totalCost, int? sourceHarvestId, string? notes)
    {
        var planting = await GetByIdAsync(id, userId);
        if (planting == null) return false;
        planting.SupplierId = supplierId; planting.StartMethod = startMethod;
        planting.Quantity = quantity; planting.TotalCost = totalCost;
        planting.SourceHarvestId = sourceHarvestId; planting.Notes = notes;
        await plantingRepository.UpdateAsync(planting);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        var planting = await GetByIdAsync(id, userId);
        if (planting == null) return false;
        await plantingRepository.DeleteAsync(id);
        return true;
    }
}
