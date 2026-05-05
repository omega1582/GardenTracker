using GardenTracker.Core.Entities;
using GardenTracker.Core.Interfaces.Repositories;
using GardenTracker.Core.Interfaces.Services;

namespace GardenTracker.Services;

public class RaisedBedService(IRaisedBedRepository bedRepository, IGardenRepository gardenRepository) : IRaisedBedService
{
    public async Task<IEnumerable<RaisedBed>> GetByGardenAsync(int gardenId, int userId)
    {
        var garden = await gardenRepository.GetByIdAsync(gardenId);
        if (garden?.UserId != userId) return [];
        return await bedRepository.GetByGardenAsync(gardenId);
    }

    public async Task<RaisedBed?> GetByIdAsync(int id, int userId)
    {
        var bed = await bedRepository.GetByIdAsync(id);
        if (bed == null) return null;
        var garden = await gardenRepository.GetByIdAsync(bed.GardenId);
        return garden?.UserId == userId ? bed : null;
    }

    public async Task<RaisedBed> CreateAsync(int gardenId, int userId, RaisedBed bed)
    {
        bed.GardenId = gardenId;
        bed.Id = await bedRepository.CreateAsync(bed);
        return bed;
    }

    public async Task<bool> UpdateAsync(int id, int userId, string name, string? material, int expectedLifespanYears, string? notes)
    {
        var bed = await GetByIdAsync(id, userId);
        if (bed == null) return false;
        bed.Name = name; bed.Material = material; bed.ExpectedLifespanYears = expectedLifespanYears; bed.Notes = notes;
        await bedRepository.UpdateAsync(bed);
        return true;
    }

    public async Task<bool> RetireAsync(int id, int userId, DateOnly retiredDate)
    {
        var bed = await GetByIdAsync(id, userId);
        if (bed == null) return false;
        await bedRepository.RetireAsync(id, retiredDate);
        return true;
    }
}
