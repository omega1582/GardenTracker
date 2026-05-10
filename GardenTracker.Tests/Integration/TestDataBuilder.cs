using Dapper;
using GardenTracker.Data;

namespace GardenTracker.Tests.Integration;

/// <summary>
/// Inserts the minimum seed rows required by FK constraints and returns
/// the generated IDs. Each test that needs a user/variety/bed calls this
/// rather than hand-rolling INSERT statements inline.
/// </summary>
public class TestDataBuilder(IConnectionFactory connectionFactory)
{
    public async Task<int> CreateUserAsync(string email = "test@example.com")
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            """
            INSERT INTO Users (Email, PasswordHash, DisplayName, CreatedAt)
            OUTPUT INSERTED.Id
            VALUES (@Email, 'hash', 'Test User', GETUTCDATE())
            """,
            new { Email = email });
    }

    public async Task<int> CreatePlantTypeAsync(string name = "Tomato")
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            "INSERT INTO PlantTypes (Name) OUTPUT INSERTED.Id VALUES (@Name)",
            new { Name = name });
    }

    public async Task<int> CreatePlantVarietyAsync(int plantTypeId, string name = "Cherokee Purple")
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            "INSERT INTO PlantVarieties (PlantTypeId, Name) OUTPUT INSERTED.Id VALUES (@PlantTypeId, @Name)",
            new { PlantTypeId = plantTypeId, Name = name });
    }

    public async Task<int> CreateGardenAsync(int userId, string name = "Test Garden")
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            "INSERT INTO Gardens (UserId, Name, CreatedAt) OUTPUT INSERTED.Id VALUES (@UserId, @Name, GETUTCDATE())",
            new { UserId = userId, Name = name });
    }

    public async Task<int> CreateSeasonAsync(int gardenId, int year = 2025)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            "INSERT INTO Seasons (GardenId, Year) OUTPUT INSERTED.Id VALUES (@GardenId, @Year)",
            new { GardenId = gardenId, Year = year });
    }

    public async Task<int> CreateBedAsync(int gardenId, string name = "Bed 1")
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            """
            INSERT INTO Beds (GardenId, Name, LengthFt, WidthFt, DepthIn, ExpectedLifespanYears, InstalledDate)
            OUTPUT INSERTED.Id
            VALUES (@GardenId, @Name, 4, 8, 12, 10, '2024-01-01')
            """,
            new { GardenId = gardenId, Name = name });
    }

    /// <summary>
    /// Creates a complete ownership chain: User → Garden → Season + Bed.
    /// Returns a record with all the IDs needed to create an inventory item or planting.
    /// </summary>
    public async Task<SeedData> CreateFullSeedAsync(string emailSuffix = "")
    {
        var userId = await CreateUserAsync($"test{emailSuffix}@example.com");
        var plantTypeId = await CreatePlantTypeAsync($"Tomato-{emailSuffix}");
        var varietyId = await CreatePlantVarietyAsync(plantTypeId, $"Cherokee Purple-{emailSuffix}");
        var gardenId = await CreateGardenAsync(userId);
        var seasonId = await CreateSeasonAsync(gardenId);
        var bedId = await CreateBedAsync(gardenId);
        return new SeedData(userId, plantTypeId, varietyId, gardenId, seasonId, bedId);
    }
}

public record SeedData(int UserId, int PlantTypeId, int VarietyId, int GardenId, int SeasonId, int BedId);
