using GardenTracker.Core.Entities;
using GardenTracker.Core.Enums;
using GardenTracker.Core.Interfaces.Repositories;
using GardenTracker.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace GardenTracker.Services;

public class ExpenseService(IExpenseRepository expenseRepository, ISeasonRepository seasonRepository, IGardenRepository gardenRepository, ILogger<ExpenseService> logger) : IExpenseService
{
    public async Task<IEnumerable<Expense>> GetBySeasonAsync(int gardenId, int year, int userId, int? bedId, ExpenseCategory? category)
    {
        var garden = await gardenRepository.GetByIdAsync(gardenId);
        if (garden?.UserId != userId)
        {
            logger.LogInformation("Expenses request for garden {GardenId} year {Year} denied — not owned by user {UserId}", gardenId, year, userId);
            return [];
        }
        return await expenseRepository.GetBySeasonAsync(gardenId, year, bedId, category);
    }

    public async Task<Expense?> GetByIdAsync(int id, int userId)
    {
        var expense = await expenseRepository.GetByIdAsync(id);
        if (expense == null) return null;
        var season = await seasonRepository.GetByIdAsync(expense.SeasonId);
        var garden = await gardenRepository.GetByIdAsync(season!.GardenId);
        if (garden?.UserId != userId)
        {
            logger.LogInformation("Expense {ExpenseId} access denied — not owned by user {UserId}", id, userId);
            return null;
        }
        return expense;
    }

    public async Task<Expense> CreateAsync(int gardenId, int year, int userId, Expense expense)
    {
        var season = await seasonRepository.GetByYearAsync(gardenId, year)
            ?? throw new InvalidOperationException($"No season found for garden {gardenId} year {year}.");
        expense.SeasonId = season.Id;
        expense.Id = await expenseRepository.CreateAsync(expense);
        logger.LogInformation("Expense {ExpenseId} created in garden {GardenId} year {Year} by user {UserId}", expense.Id, gardenId, year, userId);
        return expense;
    }

    public async Task<bool> UpdateAsync(int id, int userId, Expense updated)
    {
        var expense = await GetByIdAsync(id, userId);
        if (expense == null)
        {
            logger.LogInformation("Expense {ExpenseId} update failed — not found or not owned by user {UserId}", id, userId);
            return false;
        }
        expense.BedId = updated.BedId; expense.SupplierId = updated.SupplierId;
        expense.Category = updated.Category; expense.Description = updated.Description;
        expense.Amount = updated.Amount; expense.ExpenseDate = updated.ExpenseDate;
        await expenseRepository.UpdateAsync(expense);
        logger.LogInformation("Expense {ExpenseId} updated by user {UserId}", id, userId);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        var expense = await GetByIdAsync(id, userId);
        if (expense == null)
        {
            logger.LogInformation("Expense {ExpenseId} delete failed — not found or not owned by user {UserId}", id, userId);
            return false;
        }
        await expenseRepository.DeleteAsync(id);
        logger.LogInformation("Expense {ExpenseId} deleted by user {UserId}", id, userId);
        return true;
    }
}
