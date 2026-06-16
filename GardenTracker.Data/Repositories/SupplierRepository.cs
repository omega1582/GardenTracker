using Dapper;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Interfaces.Repositories;

namespace GardenTracker.Data.Repositories;

public class SupplierRepository(IConnectionFactory connectionFactory) : ISupplierRepository
{
    public async Task<IEnumerable<Supplier>> GetAllAsync()
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QueryAsync<Supplier>("SELECT * FROM Suppliers ORDER BY Name");
    }

    public async Task<Supplier?> GetByIdAsync(int id)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<Supplier>(
            "SELECT * FROM Suppliers WHERE Id = @Id", new { Id = id });
    }

    public async Task<Supplier?> GetByNameAsync(string name)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<Supplier>(
            "SELECT * FROM Suppliers WHERE Name = @Name", new { Name = name });
    }

    public async Task<int> CreateAsync(Supplier supplier)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            """
            INSERT INTO Suppliers (Name, SupplierType, Website, Notes)
            OUTPUT INSERTED.Id
            VALUES (@Name, @SupplierType, @Website, @Notes)
            """,
            supplier);
    }

    public async Task UpdateAsync(Supplier supplier)
    {
        using var conn = connectionFactory.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE Suppliers SET Name = @Name, SupplierType = @SupplierType, Website = @Website, Notes = @Notes WHERE Id = @Id",
            supplier);
    }
}
