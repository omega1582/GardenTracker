using GardenTracker.Core.Entities;
using GardenTracker.Core.Interfaces.Repositories;
using GardenTracker.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace GardenTracker.Services;

public class GardenService(IGardenRepository gardenRepository, ILogger<GardenService> logger) : IGardenService
{
    public Task<IEnumerable<Garden>> GetByUserAsync(int userId) =>
        gardenRepository.GetByUserAsync(userId);

    public async Task<Garden?> GetByIdAsync(int id, int userId)
    {
        var garden = await gardenRepository.GetByIdAsync(id);
        if (garden == null || garden.UserId != userId)
        {
            logger.LogInformation("Garden {GardenId} not found or not owned by user {UserId}", id, userId);
            return null;
        }
        return garden;
    }

    public async Task<Garden> CreateAsync(int userId, string name, string? location, string? notes)
    {
        var garden = new Garden { UserId = userId, Name = name, Location = location, Notes = notes, CreatedAt = DateTime.UtcNow };
        garden.Id = await gardenRepository.CreateAsync(garden);
        logger.LogInformation("Garden {GardenId} created for user {UserId}", garden.Id, userId);
        return garden;
    }

    public async Task<bool> UpdateAsync(int id, int userId, string name, string? location, string? notes)
    {
        var garden = await gardenRepository.GetByIdAsync(id);
        if (garden?.UserId != userId)
        {
            logger.LogInformation("Garden {GardenId} update failed — not found or not owned by user {UserId}", id, userId);
            return false;
        }
        garden.Name = name; garden.Location = location; garden.Notes = notes;
        await gardenRepository.UpdateAsync(garden);
        logger.LogInformation("Garden {GardenId} updated by user {UserId}", id, userId);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        var garden = await gardenRepository.GetByIdAsync(id);
        if (garden?.UserId != userId)
        {
            logger.LogInformation("Garden {GardenId} delete failed — not found or not owned by user {UserId}", id, userId);
            return false;
        }
        await gardenRepository.DeleteAsync(id);
        logger.LogInformation("Garden {GardenId} deleted by user {UserId}", id, userId);
        return true;
    }
}
