using GardenTracker.Core.Interfaces.Repositories;
using GardenTracker.Data.Context;
using GardenTracker.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GardenTracker.Data;

public static class ServiceRegistration
{
    public static IServiceCollection AddDataServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<GardenTrackerDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddSingleton<IConnectionFactory, ConnectionFactory>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IGardenRepository, GardenRepository>();
        services.AddScoped<ISeasonRepository, SeasonRepository>();
        services.AddScoped<IRaisedBedRepository, RaisedBedRepository>();
        services.AddScoped<IPlantTypeRepository, PlantTypeRepository>();
        services.AddScoped<IPlantVarietyRepository, PlantVarietyRepository>();
        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddScoped<IPlantingRepository, PlantingRepository>();
        services.AddScoped<IHarvestRepository, HarvestRepository>();
        services.AddScoped<IExpenseRepository, ExpenseRepository>();
        services.AddScoped<IMarketPriceRepository, MarketPriceRepository>();
        services.AddScoped<IWaterBillRepository, WaterBillRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();

        return services;
    }
}
