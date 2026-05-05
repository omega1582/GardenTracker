using GardenTracker.Core.Entities;

namespace GardenTracker.Core.Interfaces.Repositories;

public interface IWaterBillRepository
{
    Task<IEnumerable<WaterBill>> GetByUserAsync(int userId, int? year = null);
    Task<WaterBill?> GetByIdAsync(int id);
    Task<WaterBill?> GetByYearMonthAsync(int userId, int year, int month);
    Task<int> CreateAsync(WaterBill bill);
    Task UpdateAsync(WaterBill bill);
    Task DeleteAsync(int id);
}
