using GardenTracker.Core.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GardenTracker.Services;

public static class ServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IGardenService, GardenService>();
        services.AddScoped<ISeasonService, SeasonService>();
        services.AddScoped<IBedService, BedService>();
        services.AddScoped<IPlantTypeService, PlantTypeService>();
        services.AddScoped<IPlantVarietyService, PlantVarietyService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<IPlantingService, PlantingService>();
        services.AddScoped<IHarvestService, HarvestService>();
        services.AddScoped<IExpenseService, ExpenseService>();
        services.AddScoped<IMarketPriceService, MarketPriceService>();
        services.AddScoped<IWaterBillService, WaterBillService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<ICsvExportService, CsvExportService>();
        services.AddScoped<ICsvImportService, CsvImportService>();

        return services;
    }
}
