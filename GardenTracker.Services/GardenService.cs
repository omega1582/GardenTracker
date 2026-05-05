using GardenTracker.Core.Entities;
using GardenTracker.Core.Interfaces.Repositories;
using GardenTracker.Core.Interfaces.Services;

namespace GardenTracker.Services;

public class GardenService(IGardenRepository gardenRepository) : IGardenService
{
    public Task<IEnumerable<Garden>> GetByUserAsync(int userId) =>
        gardenRepository.GetByUserAsync(userId);

    public async Task<Garden?> GetByIdAsync(int id, int userId)
    {
        var garden = await gardenRepository.GetByIdAsync(id);
        return garden?.UserId == userId ? garden : null;
    }

    public async Task<Garden> CreateAsync(int userId, string name, string? location, string? notes)
    {
        var garden = new Garden { UserId = userId, Name = name, Location = location, Notes = notes, CreatedAt = DateTime.UtcNow };
        garden.Id = await gardenRepository.CreateAsync(garden);
        return garden;
    }

    public async Task<bool> UpdateAsync(int id, int userId, string name, string? location, string? notes)
    {
        var garden = await gardenRepository.GetByIdAsync(id);
        if (garden?.UserId != userId) return false;
        garden.Name = name; garden.Location = location; garden.Notes = notes;
        await gardenRepository.UpdateAsync(garden);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        var garden = await gardenRepository.GetByIdAsync(id);
        if (garden?.UserId != userId) return false;
        await gardenRepository.DeleteAsync(id);
        return true;
    }
}
