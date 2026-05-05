using GardenTracker.Core.Entities;

namespace GardenTracker.Core.Interfaces.Services;

public interface IRaisedBedService
{
    Task<IEnumerable<RaisedBed>> GetByGardenAsync(int gardenId, int userId);
    Task<RaisedBed?> GetByIdAsync(int id, int userId);
    Task<RaisedBed> CreateAsync(int gardenId, int userId, RaisedBed bed);
    Task<bool> UpdateAsync(int id, int userId, string name, string? material, int expectedLifespanYears, string? notes);
    Task<bool> RetireAsync(int id, int userId, DateOnly retiredDate);
}
