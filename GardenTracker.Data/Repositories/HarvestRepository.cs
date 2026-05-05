using Dapper;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Interfaces.Repositories;

namespace GardenTracker.Data.Repositories;

public class HarvestRepository(IConnectionFactory connectionFactory) : IHarvestRepository
{
    public async Task<IEnumerable<Harvest>> GetBySeasonAsync(int gardenId, int year, int? bedId = null, int? plantVarietyId = null)
    {
        using var conn = connectionFactory.CreateConnection();
        var sql = """
            SELECT h.* FROM Harvests h
            INNER JOIN Seasons s ON h.SeasonId = s.Id
            WHERE s.GardenId = @GardenId AND s.Year = @Year
              AND (@BedId IS NULL OR h.BedId = @BedId)
              AND (@PlantVarietyId IS NULL OR h.PlantVarietyId = @PlantVarietyId)
            ORDER BY h.HarvestDate
            """;
        return await conn.QueryAsync<Harvest>(sql, new { GardenId = gardenId, Year = year, BedId = bedId, PlantVarietyId = plantVarietyId });
    }

    public async Task<Harvest?> GetByIdAsync(int id)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<Harvest>(
            "SELECT * FROM Harvests WHERE Id = @Id", new { Id = id });
    }

    public async Task<int> CreateAsync(Harvest harvest)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            """
            INSERT INTO Harvests (BedId, SeasonId, PlantVarietyId, Quantity, Unit, HarvestDate, Notes)
            OUTPUT INSERTED.Id
            VALUES (@BedId, @SeasonId, @PlantVarietyId, @Quantity, @Unit, @HarvestDate, @Notes)
            """,
            harvest);
    }

    public async Task UpdateAsync(Harvest harvest)
    {
        using var conn = connectionFactory.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE Harvests SET Quantity = @Quantity, Unit = @Unit, HarvestDate = @HarvestDate, Notes = @Notes WHERE Id = @Id",
            harvest);
    }

    public async Task DeleteAsync(int id)
    {
        using var conn = connectionFactory.CreateConnection();
        await conn.ExecuteAsync("DELETE FROM Harvests WHERE Id = @Id", new { Id = id });
    }
}
