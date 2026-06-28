using GardenTracker.Core.Entities;
using GardenTracker.Core.Interfaces.Repositories;
using GardenTracker.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace GardenTracker.Services;

public class BedService(IBedRepository bedRepository, IGardenRepository gardenRepository, ILogger<BedService> logger) : IBedService
{
    public async Task<IEnumerable<Bed>> GetByGardenAsync(int gardenId, int userId)
    {
        var garden = await gardenRepository.GetByIdAsync(gardenId);
        if (garden?.UserId != userId)
        {
            logger.LogInformation("Beds request for garden {GardenId} denied — not owned by user {UserId}", gardenId, userId);
            return [];
        }
        return await bedRepository.GetByGardenAsync(gardenId);
    }

    public async Task<Bed?> GetByIdAsync(int id, int userId)
    {
        var bed = await bedRepository.GetByIdAsync(id);
        if (bed == null) return null;
        var garden = await gardenRepository.GetByIdAsync(bed.GardenId);
        if (garden?.UserId != userId)
        {
            logger.LogInformation("Bed {BedId} access denied — not owned by user {UserId}", id, userId);
            return null;
        }
        return bed;
    }

    public async Task<Bed> CreateAsync(int gardenId, int userId, Bed bed)
    {
        bed.GardenId = gardenId;
        bed.Id = await bedRepository.CreateAsync(bed);
        logger.LogInformation("Bed {BedId} created in garden {GardenId} by user {UserId}", bed.Id, gardenId, userId);
        return bed;
    }

    public async Task<bool> UpdateAsync(int id, int userId, string name, decimal lengthFt, decimal widthFt, decimal depthIn, string? material, int expectedLifespanYears, DateOnly installedDate, string? notes)
    {
        var bed = await GetByIdAsync(id, userId);
        if (bed == null)
        {
            logger.LogInformation("Bed {BedId} update failed — not found or not owned by user {UserId}", id, userId);
            return false;
        }
        bed.Name = name; bed.LengthFt = lengthFt; bed.WidthFt = widthFt; bed.DepthIn = depthIn;
        bed.Material = material; bed.ExpectedLifespanYears = expectedLifespanYears;
        bed.InstalledDate = installedDate; bed.Notes = notes;
        await bedRepository.UpdateAsync(bed);
        logger.LogInformation("Bed {BedId} updated by user {UserId}", id, userId);
        return true;
    }

    public async Task<bool> UpdatePositionAsync(int id, int userId, decimal? positionX, decimal? positionY)
    {
        var bed = await GetByIdAsync(id, userId);
        if (bed == null) return false;
        await bedRepository.UpdatePositionAsync(id, positionX, positionY);
        return true;
    }
}
