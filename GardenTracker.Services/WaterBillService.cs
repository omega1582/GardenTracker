using GardenTracker.Core.Entities;
using GardenTracker.Core.Interfaces.Repositories;
using GardenTracker.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace GardenTracker.Services;

public class WaterBillService(IWaterBillRepository waterBillRepository, ILogger<WaterBillService> logger) : IWaterBillService
{
    public Task<IEnumerable<WaterBill>> GetAllAsync(int userId, int? year = null) =>
        waterBillRepository.GetByUserAsync(userId, year);

    public async Task<WaterBill?> GetByIdAsync(int id, int userId)
    {
        var bill = await waterBillRepository.GetByIdAsync(id);
        if (bill?.UserId != userId)
        {
            logger.LogInformation("Water bill {BillId} access denied — not owned by user {UserId}", id, userId);
            return null;
        }
        return bill;
    }

    public async Task<WaterBill?> CreateAsync(int userId, WaterBill bill)
    {
        var existing = await waterBillRepository.GetByYearMonthAsync(userId, bill.Year, bill.Month);
        if (existing != null)
        {
            logger.LogInformation("Water bill creation failed — entry already exists for user {UserId} {Year}/{Month}", userId, bill.Year, bill.Month);
            return null;
        }

        bill.UserId = userId;
        bill.Id = await waterBillRepository.CreateAsync(bill);
        logger.LogInformation("Water bill {BillId} created for user {UserId} {Year}/{Month}", bill.Id, userId, bill.Year, bill.Month);
        return bill;
    }

    public async Task<bool> UpdateAsync(int id, int userId, WaterBill updated)
    {
        var bill = await waterBillRepository.GetByIdAsync(id);
        if (bill?.UserId != userId)
        {
            logger.LogInformation("Water bill {BillId} update failed — not found or not owned by user {UserId}", id, userId);
            return false;
        }

        bill.UsageCcf = updated.UsageCcf;
        bill.UsageGallons = updated.UsageGallons;
        bill.TotalCost = updated.TotalCost;
        bill.IsGardenActive = updated.IsGardenActive;
        bill.Notes = updated.Notes;
        await waterBillRepository.UpdateAsync(bill);
        logger.LogInformation("Water bill {BillId} updated by user {UserId}", id, userId);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        var bill = await waterBillRepository.GetByIdAsync(id);
        if (bill?.UserId != userId)
        {
            logger.LogInformation("Water bill {BillId} delete failed — not found or not owned by user {UserId}", id, userId);
            return false;
        }

        await waterBillRepository.DeleteAsync(id);
        logger.LogInformation("Water bill {BillId} deleted by user {UserId}", id, userId);
        return true;
    }
}
