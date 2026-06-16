using Dapper;
using GardenTracker.Data;
using GardenTracker.Data.Context;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace GardenTracker.Tests.Integration;

/// <summary>
/// Spins up a SQL Server container once for the entire test session,
/// runs EF Core migrations, and registers the Dapper DateOnly handler.
/// Shared via IClassFixture — one container per test class.
/// </summary>
public class DatabaseFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    public TestConnectionFactory ConnectionFactory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        // Run all EF Core migrations against the fresh container database
        var options = new DbContextOptionsBuilder<GardenTrackerDbContext>()
            .UseSqlServer(_container.GetConnectionString())
            .Options;

        await using var ctx = new GardenTrackerDbContext(options);
        await ctx.Database.MigrateAsync();

        // Register the DateOnly type handler once — safe to call multiple times
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
        SqlMapper.AddTypeHandler(new StringEnumTypeHandler<GardenTracker.Core.Enums.GrowthHabit>());
        SqlMapper.AddTypeHandler(new StringEnumTypeHandler<GardenTracker.Core.Enums.SunPreference>());

        ConnectionFactory = new TestConnectionFactory(_container.GetConnectionString());
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();
}
