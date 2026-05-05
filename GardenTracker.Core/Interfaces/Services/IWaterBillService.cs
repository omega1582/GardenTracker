using GardenTracker.Core.Entities;

namespace GardenTracker.Core.Interfaces.Services;

public interface IWaterBillService
{
    Task<IEnumerable<WaterBill>> GetAllAsync(int userId, int? year = null);
    Task<WaterBill?> GetByIdAsync(int id, int userId);
    Task<WaterBill?> CreateAsync(int userId, WaterBill bill);
    Task<bool> UpdateAsync(int id, int userId, WaterBill bill);
    Task<bool> DeleteAsync(int id, int userId);
}
