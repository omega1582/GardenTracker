using GardenTracker.Core.Entities;
using GardenTracker.Core.Enums;

namespace GardenTracker.Core.Interfaces.Services;

public interface IExpenseService
{
    Task<IEnumerable<Expense>> GetBySeasonAsync(int gardenId, int year, int userId, int? bedId, ExpenseCategory? category);
    Task<Expense?> GetByIdAsync(int id, int userId);
    Task<Expense> CreateAsync(int gardenId, int year, int userId, Expense expense);
    Task<bool> UpdateAsync(int id, int userId, Expense expense);
    Task<bool> DeleteAsync(int id, int userId);
}
