using GardenTracker.Core.Entities;
using GardenTracker.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace GardenTracker.Data.Context;

public class GardenTrackerDbContext(DbContextOptions<GardenTrackerDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Garden> Gardens => Set<Garden>();
    public DbSet<Bed> Beds => Set<Bed>();
    public DbSet<Season> Seasons => Set<Season>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<PlantType> PlantTypes => Set<PlantType>();
    public DbSet<PlantVariety> PlantVarieties => Set<PlantVariety>();
    public DbSet<BedPlanting> BedPlantings => Set<BedPlanting>();
    public DbSet<Harvest> Harvests => Set<Harvest>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<MarketPrice> MarketPrices => Set<MarketPrice>();
    public DbSet<WaterBill> WaterBills => Set<WaterBill>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new GardenConfiguration());
        modelBuilder.ApplyConfiguration(new BedConfiguration());
        modelBuilder.ApplyConfiguration(new SeasonConfiguration());
        modelBuilder.ApplyConfiguration(new SupplierConfiguration());
        modelBuilder.ApplyConfiguration(new PlantTypeConfiguration());
        modelBuilder.ApplyConfiguration(new PlantVarietyConfiguration());
        modelBuilder.ApplyConfiguration(new BedPlantingConfiguration());
        modelBuilder.ApplyConfiguration(new HarvestConfiguration());
        modelBuilder.ApplyConfiguration(new ExpenseConfiguration());
        modelBuilder.ApplyConfiguration(new MarketPriceConfiguration());
        modelBuilder.ApplyConfiguration(new WaterBillConfiguration());
    }
}
