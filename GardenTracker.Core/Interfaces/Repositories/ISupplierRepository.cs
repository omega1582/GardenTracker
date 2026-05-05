using GardenTracker.Core.Entities;

namespace GardenTracker.Core.Interfaces.Repositories;

public interface ISupplierRepository
{
    Task<IEnumerable<Supplier>> GetAllAsync();
    Task<Supplier?> GetByIdAsync(int id);
    Task<int> CreateAsync(Supplier supplier);
    Task UpdateAsync(Supplier supplier);
}
