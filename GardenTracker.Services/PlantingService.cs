using GardenTracker.Core.Entities;
using GardenTracker.Core.Enums;
using GardenTracker.Core.Interfaces.Repositories;
using GardenTracker.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace GardenTracker.Services;

public class PlantingService(
    IPlantingRepository plantingRepository,
    ISeasonRepository seasonRepository,
    IGardenRepository gardenRepository,
    IInventoryRepository inventoryRepository,
    ILogger<PlantingService> logger) : IPlantingService
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
        if (season == null) return null;
        var garden = await gardenRepository.GetByIdAsync(season.GardenId);
        if (garden?.UserId != userId)
        {
            logger.LogInformation("Planting {PlantingId} access denied — not owned by user {UserId}", id, userId);
            return null;
        }
        return planting;
    }

    public async Task<BedPlanting> CreateAsync(int gardenId, int year, int userId, BedPlanting planting, int? quantityUsedFromInventory)
    {
        var season = await seasonRepository.GetByYearAsync(gardenId, year)
            ?? throw new InvalidOperationException($"No season found for garden {gardenId} year {year}.");
        planting.SeasonId = season.Id;
        planting.QuantityUsedFromInventory = quantityUsedFromInventory;
        planting.Id = await plantingRepository.CreateAsync(planting);

        if (planting.InventoryItemId.HasValue && quantityUsedFromInventory.HasValue)
        {
            var item = await inventoryRepository.GetByIdAsync(planting.InventoryItemId.Value);
            if (item != null)
            {
                var newRemaining = Math.Max(0, item.QuantityRemaining - quantityUsedFromInventory.Value);
                await inventoryRepository.UpdateRemainingQuantityAsync(item.Id, newRemaining);
            }
        }

        logger.LogInformation("Planting {PlantingId} created in garden {GardenId} year {Year} by user {UserId}", planting.Id, gardenId, year, userId);
        return planting;
    }

    public async Task<bool> UpdateAsync(int id, int userId, int? supplierId, StartMethod startMethod, int quantity, decimal totalCost, int? sourceHarvestId, string? notes, int? inventoryItemId, int? quantityUsedFromInventory)
    {
        var planting = await GetByIdAsync(id, userId);
        if (planting == null)
        {
            logger.LogInformation("Planting {PlantingId} update failed — not found or not owned by user {UserId}", id, userId);
            return false;
        }

        // Restore old inventory usage before applying the new
        if (planting.InventoryItemId.HasValue && planting.QuantityUsedFromInventory.HasValue)
        {
            var oldItem = await inventoryRepository.GetByIdAsync(planting.InventoryItemId.Value);
            if (oldItem != null)
                await inventoryRepository.UpdateRemainingQuantityAsync(oldItem.Id, oldItem.QuantityRemaining + planting.QuantityUsedFromInventory.Value);
        }

        planting.SupplierId = supplierId;
        planting.StartMethod = startMethod;
        planting.Quantity = quantity;
        planting.TotalCost = totalCost;
        planting.SourceHarvestId = sourceHarvestId;
        planting.Notes = notes;
        planting.InventoryItemId = inventoryItemId;
        planting.QuantityUsedFromInventory = quantityUsedFromInventory;

        await plantingRepository.UpdateAsync(planting);

        // Apply new inventory usage
        if (inventoryItemId.HasValue && quantityUsedFromInventory.HasValue)
        {
            var newItem = await inventoryRepository.GetByIdAsync(inventoryItemId.Value);
            if (newItem != null)
            {
                var newRemaining = Math.Max(0, newItem.QuantityRemaining - quantityUsedFromInventory.Value);
                await inventoryRepository.UpdateRemainingQuantityAsync(newItem.Id, newRemaining);
            }
        }

        logger.LogInformation("Planting {PlantingId} updated by user {UserId}", id, userId);
        return true;
    }

    public async Task<bool> UpdateLayoutAsync(int id, int userId, decimal? positionX, decimal? positionY, decimal? width, decimal? height)
    {
        var planting = await GetByIdAsync(id, userId);
        if (planting == null) return false;
        await plantingRepository.UpdateLayoutAsync(id, positionX, positionY, width, height);
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

        // Restore inventory on delete
        if (planting.InventoryItemId.HasValue && planting.QuantityUsedFromInventory.HasValue)
        {
            var item = await inventoryRepository.GetByIdAsync(planting.InventoryItemId.Value);
            if (item != null)
                await inventoryRepository.UpdateRemainingQuantityAsync(item.Id, item.QuantityRemaining + planting.QuantityUsedFromInventory.Value);
        }

        await plantingRepository.DeleteAsync(id);
        logger.LogInformation("Planting {PlantingId} deleted by user {UserId}", id, userId);
        return true;
    }
}
