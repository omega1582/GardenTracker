using Dapper;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Enums;
using GardenTracker.Core.Interfaces.Repositories;

namespace GardenTracker.Data.Repositories;

public class ExpenseRepository(IConnectionFactory connectionFactory) : IExpenseRepository
{
    public async Task<IEnumerable<Expense>> GetBySeasonAsync(int gardenId, int year, int? bedId = null, ExpenseCategory? category = null)
    {
        using var conn = connectionFactory.CreateConnection();
        var sql = """
            SELECT e.*,
                   b.Name AS BedName,
                   sup.Name AS SupplierName
            FROM Expenses e
            INNER JOIN Seasons s ON e.SeasonId = s.Id
            LEFT JOIN Beds b ON e.BedId = b.Id
            LEFT JOIN Suppliers sup ON e.SupplierId = sup.Id
            WHERE s.GardenId = @GardenId AND s.Year = @Year
              AND (@BedId IS NULL OR e.BedId = @BedId)
              AND (@Category IS NULL OR e.Category = @Category)
            ORDER BY e.ExpenseDate
            """;
        return await conn.QueryAsync<Expense>(sql, new { GardenId = gardenId, Year = year, BedId = bedId, Category = category });
    }

    public async Task<Expense?> GetByIdAsync(int id)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<Expense>(
            "SELECT * FROM Expenses WHERE Id = @Id", new { Id = id });
    }

    public async Task<int> CreateAsync(Expense expense)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            """
            INSERT INTO Expenses (SeasonId, BedId, SupplierId, Category, Description, Amount, ExpenseDate)
            OUTPUT INSERTED.Id
            VALUES (@SeasonId, @BedId, @SupplierId, @Category, @Description, @Amount, @ExpenseDate)
            """,
            expense);
    }

    public async Task UpdateAsync(Expense expense)
    {
        using var conn = connectionFactory.CreateConnection();
        await conn.ExecuteAsync(
            """
            UPDATE Expenses
            SET BedId = @BedId, SupplierId = @SupplierId, Category = @Category,
                Description = @Description, Amount = @Amount, ExpenseDate = @ExpenseDate
            WHERE Id = @Id
            """,
            expense);
    }

    public async Task DeleteAsync(int id)
    {
        using var conn = connectionFactory.CreateConnection();
        await conn.ExecuteAsync("DELETE FROM Expenses WHERE Id = @Id", new { Id = id });
    }
}
