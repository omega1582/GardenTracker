using GardenTracker.Core.Entities;
using GardenTracker.Core.Enums;

namespace GardenTracker.Core.Interfaces.Services;

public interface ISupplierService
{
    Task<IEnumerable<Supplier>> GetAllAsync();
    Task<Supplier?> GetByIdAsync(int id);
    Task<Supplier> CreateAsync(string name, SupplierType type, string? website, string? notes);
    Task<bool> UpdateAsync(int id, string name, SupplierType type, string? website, string? notes);
}
