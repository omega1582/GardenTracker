using GardenTracker.Core.Entities;

namespace GardenTracker.Core.Interfaces.Repositories;

public interface IMarketPriceRepository
{
    Task<IEnumerable<MarketPrice>> GetBySeasonAsync(int gardenId, int year);
    Task<MarketPrice?> GetByIdAsync(int id);
    Task<MarketPrice?> GetEffectivePriceAsync(int seasonId, int plantVarietyId);
    Task<int> CreateAsync(MarketPrice price);
    Task UpdateAsync(MarketPrice price);
    Task DeleteAsync(int id);
}
