using GardenTracker.Core.Entities;
using GardenTracker.Core.Interfaces.Repositories;
using GardenTracker.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace GardenTracker.Services;

public class InventoryService(IInventoryRepository inventoryRepository, ILogger<InventoryService> logger) : IInventoryService
{
    public Task<IEnumerable<InventoryItem>> GetByUserAsync(int userId) =>
        inventoryRepository.GetByUserAsync(userId);

    public Task<IEnumerable<InventoryItem>> GetByVarietyAsync(int plantVarietyId, int userId) =>
        inventoryRepository.GetByVarietyAsync(plantVarietyId, userId);

    public async Task<InventoryItem?> GetByIdAsync(int id, int userId)
    {
        var item = await inventoryRepository.GetByIdAsync(id);
        if (item == null) return null;
        if (item.UserId != userId)
        {
            logger.LogInformation("Inventory item {ItemId} access denied — not owned by user {UserId}", id, userId);
            return null;
        }
        return item;
    }

    public async Task<InventoryItem> CreateAsync(int userId, InventoryItem item)
    {
        item.UserId = userId;
        item.QuantityRemaining = item.QuantityPurchased;
        item.Id = await inventoryRepository.CreateAsync(item);
        logger.LogInformation("Inventory item {ItemId} created for variety {VarietyId} by user {UserId}", item.Id, item.PlantVarietyId, userId);
        return item;
    }

    public async Task<bool> UpdateAsync(int id, int userId, int? supplierId, int quantityPurchased, decimal totalCost, DateOnly purchaseDate, string? notes)
    {
        var item = await GetByIdAsync(id, userId);
        if (item == null)
        {
            logger.LogInformation("Inventory item {ItemId} update failed — not found or not owned by user {UserId}", id, userId);
            return false;
        }

        // Adjust remaining by the same delta as the purchased quantity change
        var delta = quantityPurchased - item.QuantityPurchased;
        item.SupplierId = supplierId;
        item.QuantityPurchased = quantityPurchased;
        item.QuantityRemaining = Math.Max(0, item.QuantityRemaining + delta);
        item.TotalCost = totalCost;
        item.PurchaseDate = purchaseDate;
        item.Notes = notes;

        await inventoryRepository.UpdateAsync(item);
        await inventoryRepository.UpdateRemainingQuantityAsync(id, item.QuantityRemaining);
        logger.LogInformation("Inventory item {ItemId} updated by user {UserId}", id, userId);
        return true;
    }

    public async Task<bool> AdjustRemainingAsync(int id, int userId, int newRemaining)
    {
        var item = await GetByIdAsync(id, userId);
        if (item == null)
        {
            logger.LogInformation("Inventory item {ItemId} adjust failed — not found or not owned by user {UserId}", id, userId);
            return false;
        }
        await inventoryRepository.UpdateRemainingQuantityAsync(id, newRemaining);
        logger.LogInformation("Inventory item {ItemId} remaining adjusted to {Remaining} by user {UserId}", id, newRemaining, userId);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        var item = await GetByIdAsync(id, userId);
        if (item == null)
        {
            logger.LogInformation("Inventory item {ItemId} delete failed — not found or not owned by user {UserId}", id, userId);
            return false;
        }
        await inventoryRepository.DeleteAsync(id);
        logger.LogInformation("Inventory item {ItemId} deleted by user {UserId}", id, userId);
        return true;
    }
}
