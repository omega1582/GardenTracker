using Dapper;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Interfaces.Repositories;

namespace GardenTracker.Data.Repositories;

public class SeasonRepository(IConnectionFactory connectionFactory) : ISeasonRepository
{
    public async Task<IEnumerable<Season>> GetByGardenAsync(int gardenId)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QueryAsync<Season>(
            "SELECT * FROM Seasons WHERE GardenId = @GardenId ORDER BY Year DESC",
            new { GardenId = gardenId });
    }

    public async Task<Season?> GetByIdAsync(int id)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<Season>(
            "SELECT * FROM Seasons WHERE Id = @Id", new { Id = id });
    }

    public async Task<Season?> GetByYearAsync(int gardenId, int year)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<Season>(
            "SELECT * FROM Seasons WHERE GardenId = @GardenId AND Year = @Year",
            new { GardenId = gardenId, Year = year });
    }

    public async Task<int> CreateAsync(Season season)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            """
            INSERT INTO Seasons (GardenId, Year, Notes)
            OUTPUT INSERTED.Id
            VALUES (@GardenId, @Year, @Notes)
            """,
            season);
    }

    public async Task UpdateAsync(Season season)
    {
        using var conn = connectionFactory.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE Seasons SET Notes = @Notes WHERE Id = @Id",
            season);
    }
}
