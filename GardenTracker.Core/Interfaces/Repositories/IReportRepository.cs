using GardenTracker.Core.Models.Reports;

namespace GardenTracker.Core.Interfaces.Repositories;

public interface IReportRepository
{
    Task<IEnumerable<SeasonExpenseTotal>> GetSeasonExpenseTotalsAsync(int gardenId, int year);
    Task<IEnumerable<HarvestValueLine>> GetSeasonHarvestValuesAsync(int gardenId, int year);
    Task<IEnumerable<int>> GetSeasonYearsAsync(int gardenId);
    Task<IEnumerable<MonthlyTotal>> GetMonthlyExpenseTotalsAsync(int gardenId, int year);
    Task<IEnumerable<MonthlyTotal>> GetMonthlyHarvestValueTotalsAsync(int gardenId, int year);
}
