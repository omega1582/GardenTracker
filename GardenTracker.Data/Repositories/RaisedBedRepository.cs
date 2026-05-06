using Dapper;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Interfaces.Repositories;

namespace GardenTracker.Data.Repositories;

public class BedRepository(IConnectionFactory connectionFactory) : IBedRepository
{
    public async Task<IEnumerable<Bed>> GetByGardenAsync(int gardenId)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QueryAsync<Bed>(
            "SELECT * FROM Beds WHERE GardenId = @GardenId ORDER BY InstalledDate",
            new { GardenId = gardenId });
    }

    public async Task<Bed?> GetByIdAsync(int id)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<Bed>(
            "SELECT * FROM Beds WHERE Id = @Id",
            new { Id = id });
    }

    public async Task<int> CreateAsync(Bed bed)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            """
            INSERT INTO Beds (GardenId, Name, LengthFt, WidthFt, DepthIn, Material, ExpectedLifespanYears, InstalledDate, Notes)
            OUTPUT INSERTED.Id
            VALUES (@GardenId, @Name, @LengthFt, @WidthFt, @DepthIn, @Material, @ExpectedLifespanYears, @InstalledDate, @Notes)
            """,
            bed);
    }

    public async Task UpdateAsync(Bed bed)
    {
        using var conn = connectionFactory.CreateConnection();
        await conn.ExecuteAsync(
            """
            UPDATE Beds
            SET Name = @Name, LengthFt = @LengthFt, WidthFt = @WidthFt, DepthIn = @DepthIn,
                Material = @Material, ExpectedLifespanYears = @ExpectedLifespanYears, Notes = @Notes
            WHERE Id = @Id
            """,
            bed);
    }

}
