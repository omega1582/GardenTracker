using Dapper;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Interfaces.Repositories;

namespace GardenTracker.Data.Repositories;

public class GardenRepository(IConnectionFactory connectionFactory) : IGardenRepository
{
    public async Task<IEnumerable<Garden>> GetByUserAsync(int userId)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QueryAsync<Garden>(
            "SELECT * FROM Gardens WHERE UserId = @UserId",
            new { UserId = userId });
    }

    public async Task<Garden?> GetByIdAsync(int id)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<Garden>(
            "SELECT * FROM Gardens WHERE Id = @Id",
            new { Id = id });
    }

    public async Task<int> CreateAsync(Garden garden)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            """
            INSERT INTO Gardens (UserId, Name, Location, Notes, CreatedAt)
            OUTPUT INSERTED.Id
            VALUES (@UserId, @Name, @Location, @Notes, @CreatedAt)
            """,
            garden);
    }

    public async Task UpdateAsync(Garden garden)
    {
        using var conn = connectionFactory.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE Gardens SET Name = @Name, Location = @Location, Notes = @Notes WHERE Id = @Id",
            garden);
    }

    public async Task DeleteAsync(int id)
    {
        using var conn = connectionFactory.CreateConnection();
        await conn.ExecuteAsync("DELETE FROM Gardens WHERE Id = @Id", new { Id = id });
    }
}
