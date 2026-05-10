using GardenTracker.Core.Entities;
using GardenTracker.Core.Enums;

namespace GardenTracker.Core.Interfaces.Services;

public interface IPlantingService
{
    Task<IEnumerable<BedPlanting>> GetBySeasonAsync(int gardenId, int year, int userId, int? bedId, int? plantTypeId);
    Task<BedPlanting?> GetByIdAsync(int id, int userId);
    Task<BedPlanting> CreateAsync(int gardenId, int year, int userId, BedPlanting planting, int? quantityUsedFromInventory);
    Task<bool> UpdateAsync(int id, int userId, int? supplierId, StartMethod startMethod, int quantity, decimal totalCost, int? sourceHarvestId, string? notes, int? inventoryItemId, int? quantityUsedFromInventory);
    Task<bool> DeleteAsync(int id, int userId);
}
