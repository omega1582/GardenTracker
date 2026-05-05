using GardenTracker.Core.Entities;
using GardenTracker.Core.Interfaces.Repositories;
using GardenTracker.Core.Interfaces.Services;

namespace GardenTracker.Services;

public class WaterBillService(IWaterBillRepository waterBillRepository) : IWaterBillService
{
    public Task<IEnumerable<WaterBill>> GetAllAsync(int userId, int? year = null) =>
        waterBillRepository.GetByUserAsync(userId, year);

    public async Task<WaterBill?> GetByIdAsync(int id, int userId)
    {
        var bill = await waterBillRepository.GetByIdAsync(id);
        return bill?.UserId == userId ? bill : null;
    }

    public async Task<WaterBill?> CreateAsync(int userId, WaterBill bill)
    {
        var existing = await waterBillRepository.GetByYearMonthAsync(userId, bill.Year, bill.Month);
        if (existing != null) return null;

        bill.UserId = userId;
        bill.Id = await waterBillRepository.CreateAsync(bill);
        return bill;
    }

    public async Task<bool> UpdateAsync(int id, int userId, WaterBill updated)
    {
        var bill = await waterBillRepository.GetByIdAsync(id);
        if (bill?.UserId != userId) return false;

        bill.UsageCcf = updated.UsageCcf;
        bill.UsageGallons = updated.UsageGallons;
        bill.TotalCost = updated.TotalCost;
        bill.IsGardenActive = updated.IsGardenActive;
        bill.Notes = updated.Notes;
        await waterBillRepository.UpdateAsync(bill);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        var bill = await waterBillRepository.GetByIdAsync(id);
        if (bill?.UserId != userId) return false;

        await waterBillRepository.DeleteAsync(id);
        return true;
    }
}
