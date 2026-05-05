using Dapper;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Interfaces.Repositories;

namespace GardenTracker.Data.Repositories;

public class RaisedBedRepository(IConnectionFactory connectionFactory) : IRaisedBedRepository
{
    public async Task<IEnumerable<RaisedBed>> GetByGardenAsync(int gardenId)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QueryAsync<RaisedBed>(
            "SELECT * FROM RaisedBeds WHERE GardenId = @GardenId ORDER BY InstalledDate",
            new { GardenId = gardenId });
    }

    public async Task<RaisedBed?> GetByIdAsync(int id)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<RaisedBed>(
            "SELECT * FROM RaisedBeds WHERE Id = @Id",
            new { Id = id });
    }

    public async Task<int> CreateAsync(RaisedBed bed)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            """
            INSERT INTO RaisedBeds (GardenId, Name, LengthFt, WidthFt, DepthIn, Material, ExpectedLifespanYears, InstalledDate, Notes)
            OUTPUT INSERTED.Id
            VALUES (@GardenId, @Name, @LengthFt, @WidthFt, @DepthIn, @Material, @ExpectedLifespanYears, @InstalledDate, @Notes)
            """,
            bed);
    }

    public async Task UpdateAsync(RaisedBed bed)
    {
        using var conn = connectionFactory.CreateConnection();
        await conn.ExecuteAsync(
            """
            UPDATE RaisedBeds
            SET Name = @Name, LengthFt = @LengthFt, WidthFt = @WidthFt, DepthIn = @DepthIn,
                Material = @Material, ExpectedLifespanYears = @ExpectedLifespanYears, Notes = @Notes
            WHERE Id = @Id
            """,
            bed);
    }

    public async Task RetireAsync(int id, DateOnly retiredDate)
    {
        using var conn = connectionFactory.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE RaisedBeds SET RetiredDate = @RetiredDate WHERE Id = @Id",
            new { Id = id, RetiredDate = retiredDate });
    }
}
