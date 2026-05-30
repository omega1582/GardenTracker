using Dapper;
using FluentAssertions;
using GardenTracker.Core.Entities;
using GardenTracker.Data.Repositories;

namespace GardenTracker.Tests.Integration.Repositories;

[Trait("Category", "Integration")]
public class GardenRepositoryTests(DatabaseFixture db) : IClassFixture<DatabaseFixture>
{
    private readonly GardenRepository _repo = new(db.ConnectionFactory);
    private readonly TestDataBuilder _seed = new(db.ConnectionFactory);

    private Garden MakeGarden(int userId) => new()
    {
        UserId = userId,
        Name = "Test Garden",
        Location = "Backyard",
        Notes = "Main growing area",
        CreatedAt = DateTime.UtcNow
    };

    // ── CreateAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_InsertsGarden_AndReturnsGeneratedId()
    {
        var userId = await _seed.CreateUserAsync("grd-create@example.com");

        var id = await _repo.CreateAsync(MakeGarden(userId));

        id.Should().BeGreaterThan(0);
    }

    // ── GetByIdAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ReturnsPersistData()
    {
        var userId = await _seed.CreateUserAsync("grd-getbyid@example.com");
        var garden = MakeGarden(userId);
        garden.Id = await _repo.CreateAsync(garden);

        var fetched = await _repo.GetByIdAsync(garden.Id);

        fetched.Should().NotBeNull();
        fetched!.UserId.Should().Be(userId);
        fetched.Name.Should().Be("Test Garden");
        fetched.Location.Should().Be("Backyard");
        fetched.Notes.Should().Be("Main growing area");
        fetched.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_ForMissingId()
    {
        var result = await _repo.GetByIdAsync(999999);

        result.Should().BeNull();
    }

    // ── GetByUserAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetByUserAsync_ReturnsOnlyThatUsersGardens()
    {
        var userAId = await _seed.CreateUserAsync("grd-usera@example.com");
        var userBId = await _seed.CreateUserAsync("grd-userb@example.com");
        await _repo.CreateAsync(new Garden { UserId = userAId, Name = "A1", CreatedAt = DateTime.UtcNow });
        await _repo.CreateAsync(new Garden { UserId = userAId, Name = "A2", CreatedAt = DateTime.UtcNow });
        await _repo.CreateAsync(new Garden { UserId = userBId, Name = "B1", CreatedAt = DateTime.UtcNow });

        var results = await _repo.GetByUserAsync(userAId);

        results.Should().HaveCount(2);
        results.Should().AllSatisfy(g => g.UserId.Should().Be(userAId));
    }

    [Fact]
    public async Task GetByUserAsync_ReturnsEmpty_WhenUserHasNoGardens()
    {
        var userId = await _seed.CreateUserAsync("grd-nogardens@example.com");

        var results = await _repo.GetByUserAsync(userId);

        results.Should().BeEmpty();
    }

    // ── UpdateAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_PersistsChangedFields()
    {
        var userId = await _seed.CreateUserAsync("grd-update@example.com");
        var garden = MakeGarden(userId);
        garden.Id = await _repo.CreateAsync(garden);

        garden.Name = "Renamed Garden";
        garden.Location = "Front yard";
        garden.Notes = "New notes";
        await _repo.UpdateAsync(garden);

        var fetched = await _repo.GetByIdAsync(garden.Id);
        fetched!.Name.Should().Be("Renamed Garden");
        fetched.Location.Should().Be("Front yard");
        fetched.Notes.Should().Be("New notes");
    }

    [Fact]
    public async Task UpdateAsync_DoesNotAffectOtherGardens()
    {
        var userId = await _seed.CreateUserAsync("grd-update-isolation@example.com");
        var gardenA = new Garden { UserId = userId, Name = "Garden A", CreatedAt = DateTime.UtcNow };
        var gardenB = new Garden { UserId = userId, Name = "Garden B", CreatedAt = DateTime.UtcNow };
        gardenA.Id = await _repo.CreateAsync(gardenA);
        gardenB.Id = await _repo.CreateAsync(gardenB);

        gardenA.Name = "Garden A Updated";
        await _repo.UpdateAsync(gardenA);

        var fetchedB = await _repo.GetByIdAsync(gardenB.Id);
        fetchedB!.Name.Should().Be("Garden B");
    }

    // ── DeleteAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_RemovesGarden()
    {
        var userId = await _seed.CreateUserAsync("grd-delete@example.com");
        var garden = MakeGarden(userId);
        garden.Id = await _repo.CreateAsync(garden);

        await _repo.DeleteAsync(garden.Id);

        var fetched = await _repo.GetByIdAsync(garden.Id);
        fetched.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_CascadesToSeasonsAndBeds()
    {
        var userId = await _seed.CreateUserAsync("grd-cascade@example.com");
        var gardenId = await _seed.CreateGardenAsync(userId, "Cascade Garden");
        var seasonId = await _seed.CreateSeasonAsync(gardenId);
        var bedId = await _seed.CreateBedAsync(gardenId, "Cascade Bed");

        await _repo.DeleteAsync(gardenId);

        // Both season and bed should be gone via CASCADE
        using var conn = db.ConnectionFactory.CreateConnection();
        var seasonCount = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Seasons WHERE Id = @Id", new { Id = seasonId });
        var bedCount = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Beds WHERE Id = @Id", new { Id = bedId });

        seasonCount.Should().Be(0, "seasons should cascade-delete with their garden");
        bedCount.Should().Be(0, "beds should cascade-delete with their garden");
    }

    [Fact]
    public async Task DeleteAsync_DoesNotAffectOtherUsersGardens()
    {
        var userAId = await _seed.CreateUserAsync("grd-delete-isolation-a@example.com");
        var userBId = await _seed.CreateUserAsync("grd-delete-isolation-b@example.com");
        var gardenAId = await _repo.CreateAsync(new Garden { UserId = userAId, Name = "A", CreatedAt = DateTime.UtcNow });
        var gardenBId = await _repo.CreateAsync(new Garden { UserId = userBId, Name = "B", CreatedAt = DateTime.UtcNow });

        await _repo.DeleteAsync(gardenAId);

        var fetchedB = await _repo.GetByIdAsync(gardenBId);
        fetchedB.Should().NotBeNull();
    }
}
