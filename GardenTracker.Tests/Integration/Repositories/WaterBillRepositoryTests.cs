using FluentAssertions;
using GardenTracker.Core.Entities;
using GardenTracker.Data.Repositories;

namespace GardenTracker.Tests.Integration.Repositories;

[Trait("Category", "Integration")]
public class WaterBillRepositoryTests(DatabaseFixture db) : IClassFixture<DatabaseFixture>
{
    private readonly WaterBillRepository _repo = new(db.ConnectionFactory);
    private readonly TestDataBuilder _seed = new(db.ConnectionFactory);

    private async Task<int> CreateUserAsync(string suffix) =>
        await _seed.CreateUserAsync($"wb-{suffix}@example.com");

    private WaterBill MakeBill(int userId, int year = 2025, int month = 6) => new()
    {
        UserId = userId,
        Year = year,
        Month = month,
        UsageCcf = 4.5m,
        UsageGallons = 3366m,
        TotalCost = 68.50m,
        IsGardenActive = true,
        Notes = "Hot month"
    };

    // ── CreateAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_InsertsBill_AndReturnsGeneratedId()
    {
        var userId = await CreateUserAsync("create");

        var id = await _repo.CreateAsync(MakeBill(userId));

        id.Should().BeGreaterThan(0);
    }

    // ── GetByIdAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ReturnsPersistData()
    {
        var userId = await CreateUserAsync("getbyid");
        var bill = MakeBill(userId);
        bill.Id = await _repo.CreateAsync(bill);

        var fetched = await _repo.GetByIdAsync(bill.Id);

        fetched.Should().NotBeNull();
        fetched!.UserId.Should().Be(userId);
        fetched.Year.Should().Be(2025);
        fetched.Month.Should().Be(6);
        fetched.UsageCcf.Should().Be(4.5m);
        fetched.UsageGallons.Should().Be(3366m);
        fetched.TotalCost.Should().Be(68.50m);
        fetched.IsGardenActive.Should().BeTrue();
        fetched.Notes.Should().Be("Hot month");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_ForMissingId()
    {
        var result = await _repo.GetByIdAsync(999999);

        result.Should().BeNull();
    }

    // ── GetByUserAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetByUserAsync_ReturnsOnlyThatUserssBills()
    {
        var userAId = await CreateUserAsync("user-a");
        var userBId = await CreateUserAsync("user-b");
        await _repo.CreateAsync(MakeBill(userAId, month: 3));
        await _repo.CreateAsync(MakeBill(userAId, month: 4));
        await _repo.CreateAsync(MakeBill(userBId, month: 3));

        var results = await _repo.GetByUserAsync(userAId);

        results.Should().HaveCount(2);
        results.Should().AllSatisfy(b => b.UserId.Should().Be(userAId));
    }

    [Fact]
    public async Task GetByUserAsync_FiltersbyYear()
    {
        var userId = await CreateUserAsync("yearfilter");
        await _repo.CreateAsync(MakeBill(userId, year: 2024, month: 1));
        await _repo.CreateAsync(MakeBill(userId, year: 2025, month: 1));
        await _repo.CreateAsync(MakeBill(userId, year: 2025, month: 2));

        var results = await _repo.GetByUserAsync(userId, year: 2025);

        results.Should().HaveCount(2);
        results.Should().AllSatisfy(b => b.Year.Should().Be(2025));
    }

    [Fact]
    public async Task GetByUserAsync_ReturnsOrderedByYearDescMonthDesc()
    {
        var userId = await CreateUserAsync("order");
        await _repo.CreateAsync(MakeBill(userId, year: 2025, month: 3));
        await _repo.CreateAsync(MakeBill(userId, year: 2025, month: 1));
        await _repo.CreateAsync(MakeBill(userId, year: 2024, month: 12));

        var results = (await _repo.GetByUserAsync(userId)).ToList();

        results[0].Year.Should().Be(2025);
        results[0].Month.Should().Be(3);
        results[1].Month.Should().Be(1);
        results[2].Year.Should().Be(2024);
    }

    [Fact]
    public async Task GetByUserAsync_ReturnsEmpty_WhenUserHasNoBills()
    {
        var userId = await CreateUserAsync("nobills");

        var results = await _repo.GetByUserAsync(userId);

        results.Should().BeEmpty();
    }

    // ── GetByYearMonthAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task GetByYearMonthAsync_ReturnsBill_WhenExists()
    {
        var userId = await CreateUserAsync("yearmonth");
        await _repo.CreateAsync(MakeBill(userId, year: 2025, month: 5));

        var result = await _repo.GetByYearMonthAsync(userId, 2025, 5);

        result.Should().NotBeNull();
        result!.Month.Should().Be(5);
    }

    [Fact]
    public async Task GetByYearMonthAsync_ReturnsNull_WhenNoBillForMonth()
    {
        var userId = await CreateUserAsync("yearmonth-miss");

        var result = await _repo.GetByYearMonthAsync(userId, 2025, 5);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByYearMonthAsync_DoesNotReturnOtherUserssBill()
    {
        var userAId = await CreateUserAsync("yearmonth-usera");
        var userBId = await CreateUserAsync("yearmonth-userb");
        await _repo.CreateAsync(MakeBill(userAId, year: 2025, month: 6));

        var result = await _repo.GetByYearMonthAsync(userBId, 2025, 6);

        result.Should().BeNull();
    }

    // ── Unique index ────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_DuplicateYearMonth_ThrowsException()
    {
        var userId = await CreateUserAsync("unique");
        await _repo.CreateAsync(MakeBill(userId, year: 2025, month: 8));

        var act = async () => await _repo.CreateAsync(MakeBill(userId, year: 2025, month: 8));

        await act.Should().ThrowAsync<Exception>("unique index on (UserId, Year, Month) must be enforced");
    }

    // ── UpdateAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_PersistsChangedFields()
    {
        var userId = await CreateUserAsync("update");
        var bill = MakeBill(userId);
        bill.Id = await _repo.CreateAsync(bill);

        bill.UsageCcf = 6.0m;
        bill.UsageGallons = 4488m;
        bill.TotalCost = 89.00m;
        bill.IsGardenActive = false;
        bill.Notes = "Updated";
        await _repo.UpdateAsync(bill);

        var fetched = await _repo.GetByIdAsync(bill.Id);
        fetched!.UsageCcf.Should().Be(6.0m);
        fetched.TotalCost.Should().Be(89.00m);
        fetched.IsGardenActive.Should().BeFalse();
        fetched.Notes.Should().Be("Updated");
    }

    // ── DeleteAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_RemovesBill()
    {
        var userId = await CreateUserAsync("delete");
        var bill = MakeBill(userId);
        bill.Id = await _repo.CreateAsync(bill);

        await _repo.DeleteAsync(bill.Id);

        var fetched = await _repo.GetByIdAsync(bill.Id);
        fetched.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_DoesNotAffectOtherBills()
    {
        var userId = await CreateUserAsync("delete-isolation");
        var b1 = MakeBill(userId, month: 1);
        var b2 = MakeBill(userId, month: 2);
        b1.Id = await _repo.CreateAsync(b1);
        b2.Id = await _repo.CreateAsync(b2);

        await _repo.DeleteAsync(b1.Id);

        var fetched2 = await _repo.GetByIdAsync(b2.Id);
        fetched2.Should().NotBeNull();
    }
}
