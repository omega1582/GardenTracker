using GardenTracker.Core.Entities;
using GardenTracker.Core.Enums;
using GardenTracker.Core.Interfaces.Repositories;
using GardenTracker.Core.Interfaces.Services;

namespace GardenTracker.Services;

public class ExpenseService(IExpenseRepository expenseRepository, ISeasonRepository seasonRepository, IGardenRepository gardenRepository) : IExpenseService
{
    public async Task<IEnumerable<Expense>> GetBySeasonAsync(int gardenId, int year, int userId, int? bedId, ExpenseCategory? category)
    {
        var garden = await gardenRepository.GetByIdAsync(gardenId);
        if (garden?.UserId != userId) return [];
        return await expenseRepository.GetBySeasonAsync(gardenId, year, bedId, category);
    }

    public async Task<Expense?> GetByIdAsync(int id, int userId)
    {
        var expense = await expenseRepository.GetByIdAsync(id);
        if (expense == null) return null;
        var season = await seasonRepository.GetByIdAsync(expense.SeasonId);
        var garden = await gardenRepository.GetByIdAsync(season!.GardenId);
        return garden?.UserId == userId ? expense : null;
    }

    public async Task<Expense> CreateAsync(int gardenId, int year, int userId, Expense expense)
    {
        var season = await seasonRepository.GetByYearAsync(gardenId, year);
        expense.SeasonId = season!.Id;
        expense.Id = await expenseRepository.CreateAsync(expense);
        return expense;
    }

    public async Task<bool> UpdateAsync(int id, int userId, Expense updated)
    {
        var expense = await GetByIdAsync(id, userId);
        if (expense == null) return false;
        expense.BedId = updated.BedId; expense.SupplierId = updated.SupplierId;
        expense.Category = updated.Category; expense.Description = updated.Description;
        expense.Amount = updated.Amount; expense.ExpenseDate = updated.ExpenseDate;
        await expenseRepository.UpdateAsync(expense);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        var expense = await GetByIdAsync(id, userId);
        if (expense == null) return false;
        await expenseRepository.DeleteAsync(id);
        return true;
    }
}
