using GardenTracker.Core.Entities;
using GardenTracker.Core.Enums;

namespace GardenTracker.Core.Interfaces.Services;

public interface IPlantingService
{
    Task<IEnumerable<BedPlanting>> GetBySeasonAsync(int gardenId, int year, int userId, int? bedId, int? plantTypeId);
    Task<BedPlanting?> GetByIdAsync(int id, int userId);
    Task<BedPlanting> CreateAsync(int gardenId, int year, int userId, BedPlanting planting);
    Task<bool> UpdateAsync(int id, int userId, int? supplierId, StartMethod startMethod, int quantity, decimal totalCost, int? sourceHarvestId, string? notes);
    Task<bool> DeleteAsync(int id, int userId);
}
