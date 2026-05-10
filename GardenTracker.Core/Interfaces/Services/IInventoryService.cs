using GardenTracker.Core.Entities;

namespace GardenTracker.Core.Interfaces.Services;

public interface IInventoryService
{
    Task<IEnumerable<InventoryItem>> GetByUserAsync(int userId);
    Task<IEnumerable<InventoryItem>> GetByVarietyAsync(int plantVarietyId, int userId);
    Task<InventoryItem?> GetByIdAsync(int id, int userId);
    Task<InventoryItem> CreateAsync(int userId, InventoryItem item);
    Task<bool> UpdateAsync(int id, int userId, int? supplierId, int quantityPurchased, decimal totalCost, DateOnly purchaseDate, string? notes);
    Task<bool> AdjustRemainingAsync(int id, int userId, int newRemaining);
    Task<bool> DeleteAsync(int id, int userId);
}
