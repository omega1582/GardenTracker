using FluentAssertions;
using GardenTracker.Core.Entities;
using GardenTracker.Data.Repositories;

namespace GardenTracker.Tests.Integration.Repositories;

[Trait("Category", "Integration")]
public class BedRepositoryTests(DatabaseFixture db) : IClassFixture<DatabaseFixture>
{
    private readonly BedRepository _repo = new(db.ConnectionFactory);
    private readonly TestDataBuilder _seed = new(db.ConnectionFactory);

    private Bed MakeBed(int gardenId) => new()
    {
        GardenId = gardenId,
        Name = "Main Bed",
        LengthFt = 8,
        WidthFt = 4,
        DepthIn = 12,
        Material = "Cedar",
        ExpectedLifespanYears = 15,
        InstalledDate = new DateOnly(2023, 5, 10),
        Notes = "Good drainage"
    };

    // ── CreateAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_InsertsBed_AndReturnsGeneratedId()
    {
        var data = await _seed.CreateFullSeedAsync("bed-create");

        var id = await _repo.CreateAsync(MakeBed(data.GardenId));

        id.Should().BeGreaterThan(0);
    }

    // ── GetByIdAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ReturnsPersistData()
    {
        var data = await _seed.CreateFullSeedAsync("bed-getbyid");
        var bed = MakeBed(data.GardenId);
        bed.Id = await _repo.CreateAsync(bed);

        var fetched = await _repo.GetByIdAsync(bed.Id);

        fetched.Should().NotBeNull();
        fetched!.GardenId.Should().Be(data.GardenId);
        fetched.Name.Should().Be("Main Bed");
        fetched.LengthFt.Should().Be(8);
        fetched.WidthFt.Should().Be(4);
        fetched.DepthIn.Should().Be(12);
        fetched.Material.Should().Be("Cedar");
        fetched.ExpectedLifespanYears.Should().Be(15);
        fetched.InstalledDate.Should().Be(new DateOnly(2023, 5, 10)); // DateOnly round-trip
        fetched.Notes.Should().Be("Good drainage");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_ForMissingId()
    {
        var result = await _repo.GetByIdAsync(999999);

        result.Should().BeNull();
    }

    // ── GetByGardenAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task GetByGardenAsync_ReturnsOnlyBedsForGarden()
    {
        var dataA = await _seed.CreateFullSeedAsync("bed-garden-a");
        var dataB = await _seed.CreateFullSeedAsync("bed-garden-b");
        await _repo.CreateAsync(MakeBed(dataA.GardenId));
        await _repo.CreateAsync(MakeBed(dataA.GardenId));
        await _repo.CreateAsync(MakeBed(dataB.GardenId));

        var results = await _repo.GetByGardenAsync(dataA.GardenId);

        // +1 for the bed created by CreateFullSeedAsync
        results.Should().HaveCount(3);
        results.Should().AllSatisfy(b => b.GardenId.Should().Be(dataA.GardenId));
    }

    [Fact]
    public async Task GetByGardenAsync_ReturnsEmpty_WhenGardenHasNoBeds()
    {
        var userId = await _seed.CreateUserAsync("bed-nobeds@example.com");
        var gardenId = await _seed.CreateGardenAsync(userId, "Empty Garden");

        var results = await _repo.GetByGardenAsync(gardenId);

        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByGardenAsync_WithMultipleBeds_ReturnsAll()
    {
        var userId = await _seed.CreateUserAsync("bed-bulk@example.com");
        var gardenId = await _seed.CreateGardenAsync(userId, "Bulk Garden");
        for (var i = 1; i <= 5; i++)
            await _repo.CreateAsync(new Bed
            {
                GardenId = gardenId,
                Name = $"Bed {i}",
                LengthFt = 4, WidthFt = 4, DepthIn = 12,
                ExpectedLifespanYears = 10,
                InstalledDate = new DateOnly(2024, 1, i)
            });

        var results = await _repo.GetByGardenAsync(gardenId);

        results.Should().HaveCount(5);
    }

    // ── UpdateAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_PersistsChangedFields()
    {
        var data = await _seed.CreateFullSeedAsync("bed-update");
        var bed = MakeBed(data.GardenId);
        bed.Id = await _repo.CreateAsync(bed);

        bed.Name = "Renamed Bed";
        bed.LengthFt = 12;
        bed.WidthFt = 6;
        bed.Material = "Redwood";
        bed.Notes = "Updated notes";
        await _repo.UpdateAsync(bed);

        var fetched = await _repo.GetByIdAsync(bed.Id);
        fetched!.Name.Should().Be("Renamed Bed");
        fetched.LengthFt.Should().Be(12);
        fetched.WidthFt.Should().Be(6);
        fetched.Material.Should().Be("Redwood");
        fetched.Notes.Should().Be("Updated notes");
    }

    [Fact]
    public async Task UpdateAsync_DoesNotAffectOtherBeds()
    {
        var data = await _seed.CreateFullSeedAsync("bed-update-isolation");
        var bedA = MakeBed(data.GardenId);
        var bedB = MakeBed(data.GardenId);
        bedA.Id = await _repo.CreateAsync(bedA);
        bedB.Id = await _repo.CreateAsync(bedB);

        bedA.Name = "Updated A";
        await _repo.UpdateAsync(bedA);

        var fetchedB = await _repo.GetByIdAsync(bedB.Id);
        fetchedB!.Name.Should().Be("Main Bed");
    }
}
