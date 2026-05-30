using FluentAssertions;
using GardenTracker.Core.Entities;
using GardenTracker.Data.Repositories;

namespace GardenTracker.Tests.Integration.Repositories;

[Trait("Category", "Integration")]
public class SeasonRepositoryTests(DatabaseFixture db) : IClassFixture<DatabaseFixture>
{
    private readonly SeasonRepository _repo = new(db.ConnectionFactory);
    private readonly TestDataBuilder _seed = new(db.ConnectionFactory);

    // ── CreateAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_InsertsSeason_AndReturnsGeneratedId()
    {
        var data = await _seed.CreateFullSeedAsync("ssn-create");
        var season = new Season { GardenId = data.GardenId, Year = 2026 };

        var id = await _repo.CreateAsync(season);

        id.Should().BeGreaterThan(0);
    }

    // ── GetByIdAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ReturnsPersistData()
    {
        var data = await _seed.CreateFullSeedAsync("ssn-getbyid");
        var season = new Season { GardenId = data.GardenId, Year = 2026, Notes = "Good year" };
        season.Id = await _repo.CreateAsync(season);

        var fetched = await _repo.GetByIdAsync(season.Id);

        fetched.Should().NotBeNull();
        fetched!.GardenId.Should().Be(data.GardenId);
        fetched.Year.Should().Be(2026);
        fetched.Notes.Should().Be("Good year");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_ForMissingId()
    {
        var result = await _repo.GetByIdAsync(999999);

        result.Should().BeNull();
    }

    // ── GetByGardenAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task GetByGardenAsync_ReturnsAllSeasons_OrderedByYearDesc()
    {
        var data = await _seed.CreateFullSeedAsync("ssn-order");
        // CreateFullSeedAsync already created 2025; add more years
        await _repo.CreateAsync(new Season { GardenId = data.GardenId, Year = 2024 });
        await _repo.CreateAsync(new Season { GardenId = data.GardenId, Year = 2026 });

        var results = (await _repo.GetByGardenAsync(data.GardenId)).ToList();

        results.Should().HaveCount(3);
        results[0].Year.Should().Be(2026);
        results[1].Year.Should().Be(2025);
        results[2].Year.Should().Be(2024);
    }

    [Fact]
    public async Task GetByGardenAsync_ReturnsEmpty_WhenGardenHasNoSeasons()
    {
        var userId = await _seed.CreateUserAsync("ssn-noseasons@example.com");
        var gardenId = await _seed.CreateGardenAsync(userId, "Empty Garden");

        var results = await _repo.GetByGardenAsync(gardenId);

        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByGardenAsync_ReturnsOnlySeasonsForThatGarden()
    {
        var dataA = await _seed.CreateFullSeedAsync("ssn-garden-a");
        var dataB = await _seed.CreateFullSeedAsync("ssn-garden-b");

        var results = await _repo.GetByGardenAsync(dataA.GardenId);

        results.Should().AllSatisfy(s => s.GardenId.Should().Be(dataA.GardenId));
    }

    // ── GetByYearAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetByYearAsync_ReturnsSeason_WhenExists()
    {
        var data = await _seed.CreateFullSeedAsync("ssn-byyear");

        var result = await _repo.GetByYearAsync(data.GardenId, 2025);

        result.Should().NotBeNull();
        result!.Year.Should().Be(2025);
        result.GardenId.Should().Be(data.GardenId);
    }

    [Fact]
    public async Task GetByYearAsync_ReturnsNull_WhenNoSeasonForYear()
    {
        var data = await _seed.CreateFullSeedAsync("ssn-byyear-miss");

        var result = await _repo.GetByYearAsync(data.GardenId, 2099);

        result.Should().BeNull();
    }

    // ── UpdateAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_PersistsNotes()
    {
        var data = await _seed.CreateFullSeedAsync("ssn-update");
        var season = await _repo.GetByIdAsync(data.SeasonId);
        season!.Notes = "Updated season notes";

        await _repo.UpdateAsync(season);

        var fetched = await _repo.GetByIdAsync(data.SeasonId);
        fetched!.Notes.Should().Be("Updated season notes");
    }

    // ── Unique index ────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_DuplicateYearForSameGarden_ThrowsException()
    {
        var data = await _seed.CreateFullSeedAsync("ssn-unique");
        // 2025 was already created by CreateFullSeedAsync

        var act = async () => await _repo.CreateAsync(new Season { GardenId = data.GardenId, Year = 2025 });

        await act.Should().ThrowAsync<Exception>("unique index on (GardenId, Year) must be enforced");
    }
}
