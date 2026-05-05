using GardenTracker.Core.Entities;
using GardenTracker.Core.Enums;

namespace GardenTracker.Core.Interfaces.Repositories;

public interface IExpenseRepository
{
    Task<IEnumerable<Expense>> GetBySeasonAsync(int gardenId, int year, int? bedId = null, ExpenseCategory? category = null);
    Task<Expense?> GetByIdAsync(int id);
    Task<int> CreateAsync(Expense expense);
    Task UpdateAsync(Expense expense);
    Task DeleteAsync(int id);
}
