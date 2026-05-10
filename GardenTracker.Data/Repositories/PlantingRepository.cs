using Dapper;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Interfaces.Repositories;

namespace GardenTracker.Data.Repositories;

public class PlantingRepository(IConnectionFactory connectionFactory) : IPlantingRepository
{
    public async Task<IEnumerable<BedPlanting>> GetBySeasonAsync(int gardenId, int year, int? bedId = null, int? plantTypeId = null)
    {
        using var conn = connectionFactory.CreateConnection();
        var sql = """
            SELECT bp.*,
                   b.Name AS BedName,
                   pv.Name AS PlantVarietyName,
                   pt.Name AS PlantTypeName,
                   s2.Name AS SupplierName
            FROM BedPlantings bp
            INNER JOIN Seasons s ON bp.SeasonId = s.Id
            INNER JOIN Beds b ON bp.BedId = b.Id
            INNER JOIN PlantVarieties pv ON bp.PlantVarietyId = pv.Id
            INNER JOIN PlantTypes pt ON pv.PlantTypeId = pt.Id
            LEFT JOIN Suppliers s2 ON bp.SupplierId = s2.Id
            WHERE s.GardenId = @GardenId AND s.Year = @Year
              AND (@BedId IS NULL OR bp.BedId = @BedId)
              AND (@PlantTypeId IS NULL OR pv.PlantTypeId = @PlantTypeId)
            ORDER BY pt.Name, pv.Name
            """;
        return await conn.QueryAsync<BedPlanting>(sql, new { GardenId = gardenId, Year = year, BedId = bedId, PlantTypeId = plantTypeId });
    }

    public async Task<BedPlanting?> GetByIdAsync(int id)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<BedPlanting>(
            "SELECT * FROM BedPlantings WHERE Id = @Id", new { Id = id });
    }

    public async Task<int> CreateAsync(BedPlanting planting)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            """
            INSERT INTO BedPlantings (BedId, SeasonId, PlantVarietyId, SupplierId, StartMethod, Quantity, TotalCost, SourceHarvestId, Notes, InventoryItemId, QuantityUsedFromInventory)
            OUTPUT INSERTED.Id
            VALUES (@BedId, @SeasonId, @PlantVarietyId, @SupplierId, @StartMethod, @Quantity, @TotalCost, @SourceHarvestId, @Notes, @InventoryItemId, @QuantityUsedFromInventory)
            """,
            planting);
    }

    public async Task UpdateAsync(BedPlanting planting)
    {
        using var conn = connectionFactory.CreateConnection();
        await conn.ExecuteAsync(
            """
            UPDATE BedPlantings
            SET SupplierId = @SupplierId, StartMethod = @StartMethod, Quantity = @Quantity,
                TotalCost = @TotalCost, SourceHarvestId = @SourceHarvestId, Notes = @Notes,
                InventoryItemId = @InventoryItemId, QuantityUsedFromInventory = @QuantityUsedFromInventory
            WHERE Id = @Id
            """,
            planting);
    }

    public async Task DeleteAsync(int id)
    {
        using var conn = connectionFactory.CreateConnection();
        await conn.ExecuteAsync("DELETE FROM BedPlantings WHERE Id = @Id", new { Id = id });
    }
}
