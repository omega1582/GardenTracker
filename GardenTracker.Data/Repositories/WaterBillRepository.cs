using Dapper;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Interfaces.Repositories;

namespace GardenTracker.Data.Repositories;

public class WaterBillRepository(IConnectionFactory connectionFactory) : IWaterBillRepository
{
    public async Task<IEnumerable<WaterBill>> GetByUserAsync(int userId, int? year = null)
    {
        using var conn = connectionFactory.CreateConnection();
        var sql = """
            SELECT * FROM WaterBills
            WHERE UserId = @UserId AND (@Year IS NULL OR Year = @Year)
            ORDER BY Year DESC, Month DESC
            """;
        return await conn.QueryAsync<WaterBill>(sql, new { UserId = userId, Year = year });
    }

    public async Task<WaterBill?> GetByIdAsync(int id)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<WaterBill>(
            "SELECT * FROM WaterBills WHERE Id = @Id", new { Id = id });
    }

    public async Task<WaterBill?> GetByYearMonthAsync(int userId, int year, int month)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<WaterBill>(
            "SELECT * FROM WaterBills WHERE UserId = @UserId AND Year = @Year AND Month = @Month",
            new { UserId = userId, Year = year, Month = month });
    }

    public async Task<int> CreateAsync(WaterBill bill)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            """
            INSERT INTO WaterBills (UserId, Year, Month, UsageCcf, UsageGallons, TotalCost, IsGardenActive, Notes)
            OUTPUT INSERTED.Id
            VALUES (@UserId, @Year, @Month, @UsageCcf, @UsageGallons, @TotalCost, @IsGardenActive, @Notes)
            """,
            bill);
    }

    public async Task UpdateAsync(WaterBill bill)
    {
        using var conn = connectionFactory.CreateConnection();
        await conn.ExecuteAsync(
            """
            UPDATE WaterBills
            SET UsageCcf = @UsageCcf, UsageGallons = @UsageGallons, TotalCost = @TotalCost,
                IsGardenActive = @IsGardenActive, Notes = @Notes
            WHERE Id = @Id
            """,
            bill);
    }

    public async Task DeleteAsync(int id)
    {
        using var conn = connectionFactory.CreateConnection();
        await conn.ExecuteAsync("DELETE FROM WaterBills WHERE Id = @Id", new { Id = id });
    }
}
