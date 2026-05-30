using GardenTracker.Core.Entities;
using GardenTracker.Core.Enums;
using GardenTracker.Core.Interfaces.Repositories;
using GardenTracker.Core.Interfaces.Services;

namespace GardenTracker.Services;

public class MarketPriceService(IMarketPriceRepository marketPriceRepository, ISeasonRepository seasonRepository, IGardenRepository gardenRepository) : IMarketPriceService
{
    public async Task<IEnumerable<MarketPrice>> GetBySeasonAsync(int gardenId, int year, int userId)
    {
        var garden = await gardenRepository.GetByIdAsync(gardenId);
        if (garden?.UserId != userId) return [];
        return await marketPriceRepository.GetBySeasonAsync(gardenId, year);
    }

    public Task<MarketPrice?> GetByIdAsync(int id) => marketPriceRepository.GetByIdAsync(id);

    public async Task<MarketPrice> CreateAsync(int gardenId, int year, int userId, MarketPrice price)
    {
        var season = await seasonRepository.GetByYearAsync(gardenId, year)
            ?? throw new InvalidOperationException($"No season found for garden {gardenId} year {year}.");
        price.SeasonId = season.Id;
        price.Id = await marketPriceRepository.CreateAsync(price);
        return price;
    }

    public async Task<bool> UpdateAsync(int id, int userId, decimal pricePerUnit, HarvestUnit unit, DateOnly recordedDate)
    {
        var price = await marketPriceRepository.GetByIdAsync(id);
        if (price == null) return false;
        price.PricePerUnit = pricePerUnit; price.Unit = unit; price.RecordedDate = recordedDate;
        await marketPriceRepository.UpdateAsync(price);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        var price = await marketPriceRepository.GetByIdAsync(id);
        if (price == null) return false;
        await marketPriceRepository.DeleteAsync(id);
        return true;
    }
}
