using FluentAssertions;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Enums;
using GardenTracker.Data.Repositories;

namespace GardenTracker.Tests.Integration.Repositories;

[Trait("Category", "Integration")]
public class HarvestRepositoryTests(DatabaseFixture db) : IClassFixture<DatabaseFixture>
{
    private readonly HarvestRepository _repo = new(db.ConnectionFactory);
    private readonly TestDataBuilder _seed = new(db.ConnectionFactory);

    private Harvest MakeHarvest(SeedData data) => new()
    {
        BedId = data.BedId,
        SeasonId = data.SeasonId,
        PlantVarietyId = data.VarietyId,
        Quantity = 2.5m,
        Unit = HarvestUnit.Pounds,
        HarvestDate = new DateOnly(2025, 7, 15),
        Notes = "Good yield"
    };

    // ── CreateAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_InsertsHarvest_AndReturnsGeneratedId()
    {
        var data = await _seed.CreateFullSeedAsync("hv-create");

        var id = await _repo.CreateAsync(MakeHarvest(data));

        id.Should().BeGreaterThan(0);
    }

    // ── GetByIdAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ReturnsPersistData()
    {
        var data = await _seed.CreateFullSeedAsync("hv-getbyid");
        var harvest = MakeHarvest(data);
        harvest.Id = await _repo.CreateAsync(harvest);

        var fetched = await _repo.GetByIdAsync(harvest.Id);

        fetched.Should().NotBeNull();
        fetched!.BedId.Should().Be(data.BedId);
        fetched.SeasonId.Should().Be(data.SeasonId);
        fetched.PlantVarietyId.Should().Be(data.VarietyId);
        fetched.Quantity.Should().Be(2.5m);
        fetched.Unit.Should().Be(HarvestUnit.Pounds);
        fetched.HarvestDate.Should().Be(new DateOnly(2025, 7, 15)); // DateOnly round-trip
        fetched.Notes.Should().Be("Good yield");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_ForMissingId()
    {
        var result = await _repo.GetByIdAsync(999999);

        result.Should().BeNull();
    }

    // ── GetBySeasonAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task GetBySeasonAsync_ReturnsHarvestsForSeason()
    {
        var data = await _seed.CreateFullSeedAsync("hv-season");
        await _repo.CreateAsync(MakeHarvest(data));
        await _repo.CreateAsync(MakeHarvest(data));

        var results = await _repo.GetBySeasonAsync(data.GardenId, 2025);

        results.Should().HaveCount(2);
        results.Should().AllSatisfy(h => h.SeasonId.Should().Be(data.SeasonId));
    }

    [Fact]
    public async Task GetBySeasonAsync_ReturnsEmpty_ForWrongYear()
    {
        var data = await _seed.CreateFullSeedAsync("hv-wrongyear");
        await _repo.CreateAsync(MakeHarvest(data));

        var results = await _repo.GetBySeasonAsync(data.GardenId, 2099);

        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetBySeasonAsync_FiltersByBedId()
    {
        var data = await _seed.CreateFullSeedAsync("hv-bedfilter");
        var otherBedId = await _seed.CreateBedAsync(data.GardenId, "Bed 2");

        var h1 = MakeHarvest(data);
        var h2 = MakeHarvest(data);
        h2.BedId = otherBedId;
        await _repo.CreateAsync(h1);
        await _repo.CreateAsync(h2);

        var results = await _repo.GetBySeasonAsync(data.GardenId, 2025, bedId: data.BedId);

        results.Should().ContainSingle()
            .Which.BedId.Should().Be(data.BedId);
    }

    [Fact]
    public async Task GetBySeasonAsync_FiltersByPlantVarietyId()
    {
        var data = await _seed.CreateFullSeedAsync("hv-varfilter");
        var otherVarietyId = await _seed.CreatePlantVarietyAsync(data.PlantTypeId, "Roma-hv-varfilter");

        var h1 = MakeHarvest(data);
        var h2 = MakeHarvest(data);
        h2.PlantVarietyId = otherVarietyId;
        await _repo.CreateAsync(h1);
        await _repo.CreateAsync(h2);

        var results = await _repo.GetBySeasonAsync(data.GardenId, 2025, plantVarietyId: data.VarietyId);

        results.Should().ContainSingle()
            .Which.PlantVarietyId.Should().Be(data.VarietyId);
    }

    [Fact]
    public async Task GetBySeasonAsync_PopulatesJoinedNames()
    {
        var data = await _seed.CreateFullSeedAsync("hv-joinnames");
        await _repo.CreateAsync(MakeHarvest(data));

        var results = await _repo.GetBySeasonAsync(data.GardenId, 2025);

        var h = results.Should().ContainSingle().Subject;
        h.BedName.Should().Be("Bed 1");
        h.PlantVarietyName.Should().Be("Cherokee Purple-hv-joinnames");
        h.PlantTypeName.Should().Be("Tomato-hv-joinnames");
    }

    [Fact]
    public async Task GetBySeasonAsync_ReturnsOnlyHarvestsForThatGarden()
    {
        var dataA = await _seed.CreateFullSeedAsync("hv-isolation-a");
        var dataB = await _seed.CreateFullSeedAsync("hv-isolation-b");
        await _repo.CreateAsync(MakeHarvest(dataA));
        await _repo.CreateAsync(MakeHarvest(dataB));

        var results = await _repo.GetBySeasonAsync(dataA.GardenId, 2025);

        results.Should().ContainSingle()
            .Which.SeasonId.Should().Be(dataA.SeasonId);
    }

    // ── UpdateAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_PersistsChangedFields()
    {
        var data = await _seed.CreateFullSeedAsync("hv-update");
        var harvest = MakeHarvest(data);
        harvest.Id = await _repo.CreateAsync(harvest);

        harvest.Quantity = 5.0m;
        harvest.Unit = HarvestUnit.Ounces;
        harvest.HarvestDate = new DateOnly(2025, 8, 1);
        harvest.Notes = "Updated notes";
        await _repo.UpdateAsync(harvest);

        var fetched = await _repo.GetByIdAsync(harvest.Id);
        fetched!.Quantity.Should().Be(5.0m);
        fetched.Unit.Should().Be(HarvestUnit.Ounces);
        fetched.HarvestDate.Should().Be(new DateOnly(2025, 8, 1));
        fetched.Notes.Should().Be("Updated notes");
    }

    [Fact]
    public async Task UpdateAsync_DoesNotAffectOtherHarvests()
    {
        var data = await _seed.CreateFullSeedAsync("hv-update-isolation");
        var h1 = MakeHarvest(data);
        var h2 = MakeHarvest(data);
        h1.Id = await _repo.CreateAsync(h1);
        h2.Id = await _repo.CreateAsync(h2);

        h1.Quantity = 99m;
        await _repo.UpdateAsync(h1);

        var fetched2 = await _repo.GetByIdAsync(h2.Id);
        fetched2!.Quantity.Should().Be(2.5m);
    }

    // ── DeleteAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_RemovesHarvest()
    {
        var data = await _seed.CreateFullSeedAsync("hv-delete");
        var harvest = MakeHarvest(data);
        harvest.Id = await _repo.CreateAsync(harvest);

        await _repo.DeleteAsync(harvest.Id);

        var fetched = await _repo.GetByIdAsync(harvest.Id);
        fetched.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_DoesNotAffectOtherHarvests()
    {
        var data = await _seed.CreateFullSeedAsync("hv-delete-isolation");
        var h1 = MakeHarvest(data);
        var h2 = MakeHarvest(data);
        h1.Id = await _repo.CreateAsync(h1);
        h2.Id = await _repo.CreateAsync(h2);

        await _repo.DeleteAsync(h1.Id);

        var fetched2 = await _repo.GetByIdAsync(h2.Id);
        fetched2.Should().NotBeNull();
    }
}
