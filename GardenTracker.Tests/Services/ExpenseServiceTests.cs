using FluentAssertions;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Enums;
using GardenTracker.Core.Interfaces.Repositories;
using GardenTracker.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace GardenTracker.Tests.Services;

public class ExpenseServiceTests
{
    private readonly Mock<IExpenseRepository> _expenseRepo = new();
    private readonly Mock<ISeasonRepository> _seasonRepo = new();
    private readonly Mock<IGardenRepository> _gardenRepo = new();
    private readonly ExpenseService _sut;

    public ExpenseServiceTests() =>
        _sut = new ExpenseService(_expenseRepo.Object, _seasonRepo.Object, _gardenRepo.Object, NullLogger<ExpenseService>.Instance);

    private void SetupOwnership(Expense expense, int userId)
    {
        var season = new Season { Id = expense.SeasonId, GardenId = 1 };
        _seasonRepo.Setup(r => r.GetByIdAsync(expense.SeasonId)).ReturnsAsync(season);
        _gardenRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Garden { Id = 1, UserId = userId });
    }

    [Fact]
    public async Task GetBySeasonAsync_ReturnsEmpty_WhenUserDoesNotOwnGarden()
    {
        _gardenRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Garden { Id = 1, UserId = 42 });

        var result = await _sut.GetBySeasonAsync(gardenId: 1, year: 2025, userId: 99, null, null);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsExpense_WhenUserOwnsIt()
    {
        var expense = new Expense { Id = 1, SeasonId = 10 };
        _expenseRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(expense);
        SetupOwnership(expense, userId: 42);

        var result = await _sut.GetByIdAsync(1, userId: 42);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenUserDoesNotOwnExpense()
    {
        var expense = new Expense { Id = 1, SeasonId = 10 };
        _expenseRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(expense);
        SetupOwnership(expense, userId: 42);

        var result = await _sut.GetByIdAsync(1, userId: 99);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenExpenseNotFound()
    {
        _expenseRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Expense?)null);

        var result = await _sut.GetByIdAsync(99, userId: 42);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ThrowsInvalidOperationException_WhenSeasonDoesNotExist()
    {
        _seasonRepo.Setup(r => r.GetByYearAsync(1, 2025)).ReturnsAsync((Season?)null);

        var act = async () => await _sut.CreateAsync(gardenId: 1, year: 2025, userId: 42,
            new Expense { Category = ExpenseCategory.Seeds, Description = "test", Amount = 5m, ExpenseDate = new DateOnly(2025, 1, 1) });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No season found*");
    }

    [Fact]
    public async Task CreateAsync_AssignsSeasonId()
    {
        var season = new Season { Id = 10, GardenId = 1, Year = 2025 };
        _seasonRepo.Setup(r => r.GetByYearAsync(1, 2025)).ReturnsAsync(season);
        _expenseRepo.Setup(r => r.CreateAsync(It.IsAny<Expense>())).ReturnsAsync(20);

        var expense = new Expense
        {
            Category = ExpenseCategory.Seeds, Description = "Tomato seeds",
            Amount = 4.50m, ExpenseDate = new DateOnly(2025, 3, 1)
        };
        var result = await _sut.CreateAsync(gardenId: 1, year: 2025, userId: 42, expense);

        result.SeasonId.Should().Be(10);
        result.Id.Should().Be(20);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenExpenseNotFound()
    {
        _expenseRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Expense?)null);

        var result = await _sut.UpdateAsync(99, userId: 42, new Expense());

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenUserDoesNotOwnExpense()
    {
        var expense = new Expense { Id = 1, SeasonId = 10 };
        _expenseRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(expense);
        SetupOwnership(expense, userId: 42);

        var result = await _sut.UpdateAsync(1, userId: 99, new Expense());

        result.Should().BeFalse();
        _expenseRepo.Verify(r => r.UpdateAsync(It.IsAny<Expense>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesAllFields()
    {
        var expense = new Expense { Id = 1, SeasonId = 10, Category = ExpenseCategory.Seeds, Amount = 4.50m };
        _expenseRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(expense);
        SetupOwnership(expense, userId: 42);

        var updated = new Expense
        {
            Category = ExpenseCategory.Soil, Description = "Compost bags",
            Amount = 36.00m, BedId = 2, SupplierId = 3,
            ExpenseDate = new DateOnly(2025, 4, 10)
        };
        var result = await _sut.UpdateAsync(1, userId: 42, updated);

        result.Should().BeTrue();
        _expenseRepo.Verify(r => r.UpdateAsync(It.Is<Expense>(e =>
            e.Category == ExpenseCategory.Soil &&
            e.Amount == 36.00m &&
            e.BedId == 2)), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenExpenseNotFound()
    {
        _expenseRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Expense?)null);

        var result = await _sut.DeleteAsync(99, userId: 42);

        result.Should().BeFalse();
        _expenseRepo.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenUserDoesNotOwnExpense()
    {
        var expense = new Expense { Id = 1, SeasonId = 10 };
        _expenseRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(expense);
        SetupOwnership(expense, userId: 42);

        var result = await _sut.DeleteAsync(1, userId: 99);

        result.Should().BeFalse();
        _expenseRepo.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
    }
}
