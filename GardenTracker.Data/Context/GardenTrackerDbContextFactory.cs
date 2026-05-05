using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace GardenTracker.Data.Context;

public class GardenTrackerDbContextFactory : IDesignTimeDbContextFactory<GardenTrackerDbContext>
{
    public GardenTrackerDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../GardenTracker.Api"))
            .AddJsonFile("appsettings.json")
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<GardenTrackerDbContext>();
        optionsBuilder.UseSqlServer(config.GetConnectionString("DefaultConnection"));

        return new GardenTrackerDbContext(optionsBuilder.Options);
    }
}
