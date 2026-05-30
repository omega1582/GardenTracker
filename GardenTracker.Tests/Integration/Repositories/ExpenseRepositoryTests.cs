using FluentAssertions;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Enums;
using GardenTracker.Data.Repositories;

namespace GardenTracker.Tests.Integration.Repositories;

[Trait("Category", "Integration")]
public class ExpenseRepositoryTests(DatabaseFixture db) : IClassFixture<DatabaseFixture>
{
    private readonly ExpenseRepository _repo = new(db.ConnectionFactory);
    private readonly TestDataBuilder _seed = new(db.ConnectionFactory);

    private Expense MakeExpense(SeedData data, int? bedId = null) => new()
    {
        SeasonId = data.SeasonId,
        BedId = bedId,
        Category = ExpenseCategory.Seeds,
        Description = "Test packet of seeds",
        Amount = 4.99m,
        ExpenseDate = new DateOnly(2025, 3, 20)
    };

    // ── CreateAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_InsertsExpense_AndReturnsGeneratedId()
    {
        var data = await _seed.CreateFullSeedAsync("exp-create");

        var id = await _repo.CreateAsync(MakeExpense(data));

        id.Should().BeGreaterThan(0);
    }

    // ── GetByIdAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ReturnsPersistData()
    {
        var data = await _seed.CreateFullSeedAsync("exp-getbyid");
        var expense = MakeExpense(data);
        expense.Id = await _repo.CreateAsync(expense);

        var fetched = await _repo.GetByIdAsync(expense.Id);

        fetched.Should().NotBeNull();
        fetched!.SeasonId.Should().Be(data.SeasonId);
        fetched.Category.Should().Be(ExpenseCategory.Seeds);
        fetched.Description.Should().Be("Test packet of seeds");
        fetched.Amount.Should().Be(4.99m);              // decimal precision
        fetched.ExpenseDate.Should().Be(new DateOnly(2025, 3, 20)); // DateOnly round-trip
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_ForMissingId()
    {
        var result = await _repo.GetByIdAsync(999999);

        result.Should().BeNull();
    }

    // ── GetBySeasonAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task GetBySeasonAsync_ReturnsExpensesForSeason()
    {
        var data = await _seed.CreateFullSeedAsync("exp-season");
        await _seed.CreateExpensesAsync(data.SeasonId, 3);

        var results = await _repo.GetBySeasonAsync(data.GardenId, 2025);

        results.Should().HaveCount(3);
        results.Should().AllSatisfy(e => e.SeasonId.Should().Be(data.SeasonId));
    }

    [Fact]
    public async Task GetBySeasonAsync_ReturnsEmpty_WhenNoExpenses()
    {
        var data = await _seed.CreateFullSeedAsync("exp-empty");

        var results = await _repo.GetBySeasonAsync(data.GardenId, 2025);

        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetBySeasonAsync_FiltersByBedId()
    {
        var data = await _seed.CreateFullSeedAsync("exp-bedfilter");
        var otherBedId = await _seed.CreateBedAsync(data.GardenId, "Bed 2");
        await _repo.CreateAsync(MakeExpense(data, bedId: data.BedId));
        await _repo.CreateAsync(MakeExpense(data, bedId: otherBedId));
        await _repo.CreateAsync(MakeExpense(data, bedId: null));

        var results = await _repo.GetBySeasonAsync(data.GardenId, 2025, bedId: data.BedId);

        results.Should().ContainSingle()
            .Which.BedId.Should().Be(data.BedId);
    }

    [Fact]
    public async Task GetBySeasonAsync_FiltersByCategory()
    {
        var data = await _seed.CreateFullSeedAsync("exp-catfilter");
        await _repo.CreateAsync(new Expense
        {
            SeasonId = data.SeasonId, Category = ExpenseCategory.Seeds,
            Description = "Seeds", Amount = 5.00m, ExpenseDate = new DateOnly(2025, 1, 1)
        });
        await _repo.CreateAsync(new Expense
        {
            SeasonId = data.SeasonId, Category = ExpenseCategory.Soil,
            Description = "Soil bag", Amount = 12.00m, ExpenseDate = new DateOnly(2025, 1, 2)
        });
        await _repo.CreateAsync(new Expense
        {
            SeasonId = data.SeasonId, Category = ExpenseCategory.Soil,
            Description = "More soil", Amount = 8.00m, ExpenseDate = new DateOnly(2025, 1, 3)
        });

        var results = await _repo.GetBySeasonAsync(data.GardenId, 2025, category: ExpenseCategory.Soil);

        results.Should().HaveCount(2);
        results.Should().AllSatisfy(e => e.Category.Should().Be(ExpenseCategory.Soil));
    }

    [Fact]
    public async Task GetBySeasonAsync_PopulatesJoinedNames()
    {
        var data = await _seed.CreateFullSeedAsync("exp-joinnames");
        var supplierId = await _seed.CreateSupplierAsync("Supplier-exp-joinnames");
        var expense = new Expense
        {
            SeasonId = data.SeasonId,
            BedId = data.BedId,
            SupplierId = supplierId,
            Category = ExpenseCategory.Seeds,
            Description = "Seeds with supplier",
            Amount = 6.00m,
            ExpenseDate = new DateOnly(2025, 4, 1)
        };
        await _repo.CreateAsync(expense);

        var results = await _repo.GetBySeasonAsync(data.GardenId, 2025);

        var e = results.Should().ContainSingle().Subject;
        e.BedName.Should().Be("Bed 1");
        e.SupplierName.Should().Be("Supplier-exp-joinnames");
    }

    [Fact]
    public async Task GetBySeasonAsync_ReturnsOnlyExpensesForThatGardenYear()
    {
        var dataA = await _seed.CreateFullSeedAsync("exp-isolation-a");
        var dataB = await _seed.CreateFullSeedAsync("exp-isolation-b");
        await _repo.CreateAsync(MakeExpense(dataA));
        await _repo.CreateAsync(MakeExpense(dataB));

        var results = await _repo.GetBySeasonAsync(dataA.GardenId, 2025);

        results.Should().ContainSingle()
            .Which.SeasonId.Should().Be(dataA.SeasonId);
    }

    [Fact]
    public async Task GetBySeasonAsync_WithBulkExpenses_ReturnsAll()
    {
        var data = await _seed.CreateFullSeedAsync("exp-bulk");
        await _seed.CreateExpensesAsync(data.SeasonId, 10);

        var results = await _repo.GetBySeasonAsync(data.GardenId, 2025);

        results.Should().HaveCount(10);
    }

    // ── UpdateAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_PersistsChangedFields()
    {
        var data = await _seed.CreateFullSeedAsync("exp-update");
        var expense = MakeExpense(data);
        expense.Id = await _repo.CreateAsync(expense);

        expense.Category = ExpenseCategory.Fertilizer;
        expense.Description = "Updated description";
        expense.Amount = 19.99m;
        expense.ExpenseDate = new DateOnly(2025, 6, 15);
        await _repo.UpdateAsync(expense);

        var fetched = await _repo.GetByIdAsync(expense.Id);
        fetched!.Category.Should().Be(ExpenseCategory.Fertilizer);
        fetched.Description.Should().Be("Updated description");
        fetched.Amount.Should().Be(19.99m);
        fetched.ExpenseDate.Should().Be(new DateOnly(2025, 6, 15));
    }

    [Fact]
    public async Task UpdateAsync_DoesNotAffectOtherExpenses()
    {
        var data = await _seed.CreateFullSeedAsync("exp-update-isolation");
        var expA = MakeExpense(data);
        var expB = MakeExpense(data);
        expA.Id = await _repo.CreateAsync(expA);
        expB.Id = await _repo.CreateAsync(expB);

        expA.Amount = 99.00m;
        await _repo.UpdateAsync(expA);

        var fetchedB = await _repo.GetByIdAsync(expB.Id);
        fetchedB!.Amount.Should().Be(4.99m);
    }

    // ── DeleteAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_RemovesExpense()
    {
        var data = await _seed.CreateFullSeedAsync("exp-delete");
        var expense = MakeExpense(data);
        expense.Id = await _repo.CreateAsync(expense);

        await _repo.DeleteAsync(expense.Id);

        var fetched = await _repo.GetByIdAsync(expense.Id);
        fetched.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_DoesNotAffectOtherExpenses()
    {
        var data = await _seed.CreateFullSeedAsync("exp-delete-isolation");
        var expA = MakeExpense(data);
        var expB = MakeExpense(data);
        expA.Id = await _repo.CreateAsync(expA);
        expB.Id = await _repo.CreateAsync(expB);

        await _repo.DeleteAsync(expA.Id);

        var fetchedB = await _repo.GetByIdAsync(expB.Id);
        fetchedB.Should().NotBeNull();
    }
}
