using GardenTracker.Core.Entities;
using GardenTracker.Core.Enums;

namespace GardenTracker.Core.Interfaces.Services;

public interface IHarvestService
{
    Task<IEnumerable<Harvest>> GetBySeasonAsync(int gardenId, int year, int userId, int? bedId, int? plantVarietyId);
    Task<Harvest?> GetByIdAsync(int id, int userId);
    Task<Harvest> CreateAsync(int gardenId, int year, int userId, Harvest harvest);
    Task<bool> UpdateAsync(int id, int userId, decimal quantity, HarvestUnit unit, DateOnly harvestDate, string? notes);
    Task<bool> DeleteAsync(int id, int userId);
}
