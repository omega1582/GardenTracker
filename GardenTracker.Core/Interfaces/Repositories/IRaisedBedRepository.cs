using GardenTracker.Core.Entities;

namespace GardenTracker.Core.Interfaces.Repositories;

public interface IRaisedBedRepository
{
    Task<IEnumerable<RaisedBed>> GetByGardenAsync(int gardenId);
    Task<RaisedBed?> GetByIdAsync(int id);
    Task<int> CreateAsync(RaisedBed bed);
    Task UpdateAsync(RaisedBed bed);
    Task RetireAsync(int id, DateOnly retiredDate);
}
