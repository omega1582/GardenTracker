using GardenTracker.Core.Entities;
using GardenTracker.Core.Enums;

namespace GardenTracker.Core.Interfaces.Services;

public interface IMarketPriceService
{
    Task<IEnumerable<MarketPrice>> GetBySeasonAsync(int gardenId, int year, int userId);
    Task<MarketPrice?> GetByIdAsync(int id);
    Task<MarketPrice> CreateAsync(int gardenId, int year, int userId, MarketPrice price);
    Task<bool> UpdateAsync(int id, int userId, decimal pricePerUnit, HarvestUnit unit, DateOnly recordedDate);
    Task<bool> DeleteAsync(int id, int userId);
}
