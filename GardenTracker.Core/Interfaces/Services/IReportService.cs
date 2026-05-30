using GardenTracker.Core.Models.Reports;

namespace GardenTracker.Core.Interfaces.Services;

public interface IReportService
{
    Task<SeasonSummaryResult?> GetSeasonSummaryAsync(int gardenId, int year, int userId);
    Task<IEnumerable<WaterAttributionResult>> GetWaterAttributionAsync(int userId, int? year);
    Task<IEnumerable<YearSummaryResult>?> GetYearOverYearAsync(int gardenId, int userId);
    Task<IEnumerable<BedBreakdownResult>?> GetBedBreakdownAsync(int gardenId, int year, int userId);
}
