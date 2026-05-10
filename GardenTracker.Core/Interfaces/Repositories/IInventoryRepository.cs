using GardenTracker.Core.Entities;

namespace GardenTracker.Core.Interfaces.Repositories;

public interface IInventoryRepository
{
    Task<IEnumerable<InventoryItem>> GetByUserAsync(int userId);
    Task<IEnumerable<InventoryItem>> GetByVarietyAsync(int plantVarietyId, int userId);
    Task<InventoryItem?> GetByIdAsync(int id);
    Task<int> CreateAsync(InventoryItem item);
    Task UpdateAsync(InventoryItem item);
    Task UpdateRemainingQuantityAsync(int id, int remaining);
    Task DeleteAsync(int id);
}
