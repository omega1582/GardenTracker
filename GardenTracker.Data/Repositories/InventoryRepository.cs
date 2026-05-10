using Dapper;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Interfaces.Repositories;

namespace GardenTracker.Data.Repositories;

public class InventoryRepository(IConnectionFactory connectionFactory) : IInventoryRepository
{
    public async Task<IEnumerable<InventoryItem>> GetByUserAsync(int userId)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QueryAsync<InventoryItem>(
            """
            SELECT i.*, pv.Name AS PlantVarietyName, pt.Name AS PlantTypeName, s.Name AS SupplierName
            FROM InventoryItems i
            INNER JOIN PlantVarieties pv ON i.PlantVarietyId = pv.Id
            INNER JOIN PlantTypes pt ON pv.PlantTypeId = pt.Id
            LEFT JOIN Suppliers s ON i.SupplierId = s.Id
            WHERE i.UserId = @UserId
            ORDER BY pt.Name, pv.Name, i.PurchaseDate
            """,
            new { UserId = userId });
    }

    public async Task<IEnumerable<InventoryItem>> GetByVarietyAsync(int plantVarietyId, int userId)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QueryAsync<InventoryItem>(
            """
            SELECT * FROM InventoryItems
            WHERE PlantVarietyId = @PlantVarietyId AND UserId = @UserId
            ORDER BY PurchaseDate
            """,
            new { PlantVarietyId = plantVarietyId, UserId = userId });
    }

    public async Task<InventoryItem?> GetByIdAsync(int id)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<InventoryItem>(
            "SELECT * FROM InventoryItems WHERE Id = @Id", new { Id = id });
    }

    public async Task<int> CreateAsync(InventoryItem item)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            """
            INSERT INTO InventoryItems (UserId, PlantVarietyId, SupplierId, Type, QuantityPurchased, QuantityRemaining, TotalCost, PurchaseDate, Notes)
            OUTPUT INSERTED.Id
            VALUES (@UserId, @PlantVarietyId, @SupplierId, @Type, @QuantityPurchased, @QuantityRemaining, @TotalCost, @PurchaseDate, @Notes)
            """,
            item);
    }

    public async Task UpdateAsync(InventoryItem item)
    {
        using var conn = connectionFactory.CreateConnection();
        await conn.ExecuteAsync(
            """
            UPDATE InventoryItems
            SET SupplierId = @SupplierId, QuantityPurchased = @QuantityPurchased,
                TotalCost = @TotalCost, PurchaseDate = @PurchaseDate, Notes = @Notes
            WHERE Id = @Id
            """,
            item);
    }

    public async Task UpdateRemainingQuantityAsync(int id, int remaining)
    {
        using var conn = connectionFactory.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE InventoryItems SET QuantityRemaining = @Remaining WHERE Id = @Id",
            new { Id = id, Remaining = remaining });
    }

    public async Task DeleteAsync(int id)
    {
        using var conn = connectionFactory.CreateConnection();
        await conn.ExecuteAsync("DELETE FROM InventoryItems WHERE Id = @Id", new { Id = id });
    }
}
