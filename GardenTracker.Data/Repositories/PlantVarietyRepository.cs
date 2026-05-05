using Dapper;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Interfaces.Repositories;

namespace GardenTracker.Data.Repositories;

public class PlantVarietyRepository(IConnectionFactory connectionFactory) : IPlantVarietyRepository
{
    public async Task<IEnumerable<PlantVariety>> GetByPlantTypeAsync(int plantTypeId)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QueryAsync<PlantVariety>(
            "SELECT * FROM PlantVarieties WHERE PlantTypeId = @PlantTypeId ORDER BY Name",
            new { PlantTypeId = plantTypeId });
    }

    public async Task<PlantVariety?> GetByIdAsync(int id)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<PlantVariety>(
            "SELECT * FROM PlantVarieties WHERE Id = @Id", new { Id = id });
    }

    public async Task<int> CreateAsync(PlantVariety variety)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            """
            INSERT INTO PlantVarieties (PlantTypeId, Name, Notes)
            OUTPUT INSERTED.Id
            VALUES (@PlantTypeId, @Name, @Notes)
            """,
            variety);
    }

    public async Task UpdateAsync(PlantVariety variety)
    {
        using var conn = connectionFactory.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE PlantVarieties SET Name = @Name, Notes = @Notes WHERE Id = @Id", variety);
    }
}
