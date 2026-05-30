using Dapper;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Interfaces.Repositories;

namespace GardenTracker.Data.Repositories;

public class MarketPriceRepository(IConnectionFactory connectionFactory) : IMarketPriceRepository
{
    public async Task<IEnumerable<MarketPrice>> GetBySeasonAsync(int gardenId, int year)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QueryAsync<MarketPrice>(
            """
            SELECT mp.*,
                   pt.Name AS PlantTypeName,
                   pv.Name AS PlantVarietyName
            FROM MarketPrices mp
            INNER JOIN Seasons s ON mp.SeasonId = s.Id
            INNER JOIN PlantTypes pt ON mp.PlantTypeId = pt.Id
            LEFT JOIN PlantVarieties pv ON mp.PlantVarietyId = pv.Id
            WHERE s.GardenId = @GardenId AND s.Year = @Year
            ORDER BY pt.Name, pv.Name
            """,
            new { GardenId = gardenId, Year = year });
    }

    public async Task<MarketPrice?> GetByIdAsync(int id)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<MarketPrice>(
            "SELECT * FROM MarketPrices WHERE Id = @Id", new { Id = id });
    }

    public async Task<MarketPrice?> GetEffectivePriceAsync(int seasonId, int plantVarietyId)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<MarketPrice>(
            """
            SELECT TOP 1 mp.* FROM MarketPrices mp
            INNER JOIN PlantVarieties pv ON pv.Id = @PlantVarietyId
            WHERE mp.SeasonId = @SeasonId
              AND mp.PlantTypeId = pv.PlantTypeId
              AND (mp.PlantVarietyId = @PlantVarietyId OR mp.PlantVarietyId IS NULL)
            ORDER BY mp.PlantVarietyId DESC
            """,
            new { SeasonId = seasonId, PlantVarietyId = plantVarietyId });
    }

    public async Task<int> CreateAsync(MarketPrice price)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            """
            INSERT INTO MarketPrices (SeasonId, PlantTypeId, PlantVarietyId, PricePerUnit, Unit, RecordedDate)
            OUTPUT INSERTED.Id
            VALUES (@SeasonId, @PlantTypeId, @PlantVarietyId, @PricePerUnit, @Unit, @RecordedDate)
            """,
            price);
    }

    public async Task UpdateAsync(MarketPrice price)
    {
        using var conn = connectionFactory.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE MarketPrices SET PricePerUnit = @PricePerUnit, Unit = @Unit, RecordedDate = @RecordedDate WHERE Id = @Id",
            price);
    }

    public async Task DeleteAsync(int id)
    {
        using var conn = connectionFactory.CreateConnection();
        await conn.ExecuteAsync("DELETE FROM MarketPrices WHERE Id = @Id", new { Id = id });
    }
}
