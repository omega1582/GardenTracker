using GardenTracker.Core.Entities;
using GardenTracker.Core.Enums;
using GardenTracker.Core.Interfaces.Repositories;
using GardenTracker.Core.Interfaces.Services;

namespace GardenTracker.Services;

public class SupplierService(ISupplierRepository supplierRepository) : ISupplierService
{
    public Task<IEnumerable<Supplier>> GetAllAsync() => supplierRepository.GetAllAsync();

    public Task<Supplier?> GetByIdAsync(int id) => supplierRepository.GetByIdAsync(id);

    public async Task<Supplier> CreateAsync(string name, SupplierType type, string? website, string? notes)
    {
        var supplier = new Supplier { Name = name, SupplierType = type, Website = website, Notes = notes };
        supplier.Id = await supplierRepository.CreateAsync(supplier);
        return supplier;
    }

    public async Task<bool> UpdateAsync(int id, string name, SupplierType type, string? website, string? notes)
    {
        var supplier = await supplierRepository.GetByIdAsync(id);
        if (supplier == null) return false;
        supplier.Name = name; supplier.SupplierType = type; supplier.Website = website; supplier.Notes = notes;
        await supplierRepository.UpdateAsync(supplier);
        return true;
    }
}
