using Dapper;
using GardenTracker.Core.Interfaces.Repositories;
using GardenTracker.Core.Models.Reports;

namespace GardenTracker.Data.Repositories;

public class ReportRepository(IConnectionFactory connectionFactory) : IReportRepository
{
    public async Task<IEnumerable<SeasonExpenseTotal>> GetSeasonExpenseTotalsAsync(int gardenId, int year)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QueryAsync<SeasonExpenseTotal>(
            """
            SELECT e.Category, SUM(e.Amount) AS Total
            FROM Expenses e
            JOIN Seasons s ON e.SeasonId = s.Id
            WHERE s.GardenId = @GardenId AND s.Year = @Year
            GROUP BY e.Category
            """,
            new { GardenId = gardenId, Year = year });
    }

    public async Task<IEnumerable<HarvestValueLine>> GetSeasonHarvestValuesAsync(int gardenId, int year)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QueryAsync<HarvestValueLine>(
            """
            SELECT
                pv.Name AS VarietyName,
                pt.Name AS PlantTypeName,
                h.Quantity,
                h.Unit,
                COALESCE(
                    (SELECT TOP 1 PricePerUnit FROM MarketPrices
                     WHERE SeasonId = s.Id AND PlantVarietyId = h.PlantVarietyId AND Unit = h.Unit
                     ORDER BY RecordedDate DESC),
                    (SELECT TOP 1 PricePerUnit FROM MarketPrices
                     WHERE SeasonId = s.Id AND PlantTypeId = pv.PlantTypeId AND PlantVarietyId IS NULL AND Unit = h.Unit
                     ORDER BY RecordedDate DESC)
                ) AS PricePerUnit
            FROM Harvests h
            JOIN Seasons s ON h.SeasonId = s.Id
            JOIN PlantVarieties pv ON h.PlantVarietyId = pv.Id
            JOIN PlantTypes pt ON pv.PlantTypeId = pt.Id
            WHERE s.GardenId = @GardenId AND s.Year = @Year
            """,
            new { GardenId = gardenId, Year = year });
    }

    public async Task<IEnumerable<int>> GetSeasonYearsAsync(int gardenId)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QueryAsync<int>(
            "SELECT Year FROM Seasons WHERE GardenId = @GardenId ORDER BY Year DESC",
            new { GardenId = gardenId });
    }

    public async Task<IEnumerable<MonthlyTotal>> GetMonthlyExpenseTotalsAsync(int gardenId, int year)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QueryAsync<MonthlyTotal>(
            """
            SELECT MONTH(e.ExpenseDate) AS Month, SUM(e.Amount) AS Total
            FROM Expenses e
            JOIN Seasons s ON e.SeasonId = s.Id
            WHERE s.GardenId = @GardenId AND s.Year = @Year
            GROUP BY MONTH(e.ExpenseDate)
            """,
            new { GardenId = gardenId, Year = year });
    }

    public async Task<IEnumerable<MonthlyTotal>> GetMonthlyHarvestValueTotalsAsync(int gardenId, int year)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QueryAsync<MonthlyTotal>(
            """
            SELECT
                MONTH(h.HarvestDate) AS Month,
                SUM(h.Quantity * COALESCE(
                    (SELECT TOP 1 PricePerUnit FROM MarketPrices
                     WHERE SeasonId = s.Id AND PlantVarietyId = h.PlantVarietyId AND Unit = h.Unit
                     ORDER BY RecordedDate DESC),
                    (SELECT TOP 1 PricePerUnit FROM MarketPrices
                     WHERE SeasonId = s.Id AND PlantTypeId = pv.PlantTypeId AND PlantVarietyId IS NULL AND Unit = h.Unit
                     ORDER BY RecordedDate DESC),
                    0
                )) AS Total
            FROM Harvests h
            JOIN Seasons s ON h.SeasonId = s.Id
            JOIN PlantVarieties pv ON h.PlantVarietyId = pv.Id
            WHERE s.GardenId = @GardenId AND s.Year = @Year
            GROUP BY MONTH(h.HarvestDate)
            """,
            new { GardenId = gardenId, Year = year });
    }
}
