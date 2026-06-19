using Dapper;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Interfaces.Repositories;

namespace GardenTracker.Data.Repositories;

public class PlantTypeRepository(IConnectionFactory connectionFactory) : IPlantTypeRepository
{
    public async Task<IEnumerable<PlantType>> GetAllAsync()
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QueryAsync<PlantType>("SELECT * FROM PlantTypes ORDER BY Name");
    }

    public async Task<PlantType?> GetByIdAsync(int id)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<PlantType>(
            "SELECT * FROM PlantTypes WHERE Id = @Id", new { Id = id });
    }

    public async Task<PlantType?> GetByNameAsync(string name)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<PlantType>(
            "SELECT * FROM PlantTypes WHERE Name = @Name", new { Name = name });
    }

    public async Task<int> CreateAsync(PlantType plantType)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            """
            INSERT INTO PlantTypes (Name, Category, GrowthHabit, DaysToMaturity, SpacingInches, SunPreference, IsPerennial)
            OUTPUT INSERTED.Id
            VALUES (@Name, @Category, @GrowthHabit, @DaysToMaturity, @SpacingInches, @SunPreference, @IsPerennial)
            """,
            plantType);
    }

    public async Task UpdateAsync(PlantType plantType)
    {
        using var conn = connectionFactory.CreateConnection();
        await conn.ExecuteAsync(
            """
            UPDATE PlantTypes
            SET Name = @Name, Category = @Category, GrowthHabit = @GrowthHabit, DaysToMaturity = @DaysToMaturity,
                SpacingInches = @SpacingInches, SunPreference = @SunPreference, IsPerennial = @IsPerennial
            WHERE Id = @Id
            """,
            plantType);
    }
}
