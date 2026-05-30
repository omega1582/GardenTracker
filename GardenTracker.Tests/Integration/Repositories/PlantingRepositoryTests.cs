using Dapper;
using FluentAssertions;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Enums;
using GardenTracker.Data.Repositories;

namespace GardenTracker.Tests.Integration.Repositories;

[Trait("Category", "Integration")]
public class PlantingRepositoryTests(DatabaseFixture db) : IClassFixture<DatabaseFixture>
{
    private readonly PlantingRepository _repo = new(db.ConnectionFactory);
    private readonly InventoryRepository _inventoryRepo = new(db.ConnectionFactory);
    private readonly TestDataBuilder _seed = new(db.ConnectionFactory);

    private BedPlanting MakePlanting(SeedData data, int? inventoryItemId = null, int? qtyFromInventory = null) => new()
    {
        BedId = data.BedId,
        SeasonId = data.SeasonId,
        PlantVarietyId = data.VarietyId,
        StartMethod = StartMethod.Seed,
        Quantity = 6,
        TotalCost = 0m,
        InventoryItemId = inventoryItemId,
        QuantityUsedFromInventory = qtyFromInventory
    };

    private InventoryItem MakeInventoryItem(SeedData data) => new()
    {
        UserId = data.UserId,
        PlantVarietyId = data.VarietyId,
        Type = InventoryType.Seed,
        QuantityPurchased = 50,
        QuantityRemaining = 50,
        TotalCost = 3.99m,
        PurchaseDate = new DateOnly(2025, 3, 1)
    };

    // ── CreateAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_InsertsPlanting_AndReturnsId()
    {
        var data = await _seed.CreateFullSeedAsync("plt-create");
        var planting = MakePlanting(data);

        var id = await _repo.CreateAsync(planting);

        id.Should().BeGreaterThan(0);
    }

    // ── GetByIdAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ReturnsPersistData()
    {
        var data = await _seed.CreateFullSeedAsync("plt-getbyid");
        var planting = MakePlanting(data);
        planting.Id = await _repo.CreateAsync(planting);

        var fetched = await _repo.GetByIdAsync(planting.Id);

        fetched.Should().NotBeNull();
        fetched!.BedId.Should().Be(data.BedId);
        fetched.SeasonId.Should().Be(data.SeasonId);
        fetched.PlantVarietyId.Should().Be(data.VarietyId);
        fetched.Quantity.Should().Be(6);
        fetched.StartMethod.Should().Be(StartMethod.Seed);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_ForMissingId()
    {
        var result = await _repo.GetByIdAsync(999999);

        result.Should().BeNull();
    }

    // ── GetBySeasonAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task GetBySeasonAsync_FiltersCorrectly()
    {
        var data = await _seed.CreateFullSeedAsync("plt-season");
        await _repo.CreateAsync(MakePlanting(data));
        await _repo.CreateAsync(MakePlanting(data));

        var results = await _repo.GetBySeasonAsync(data.GardenId, 2025);

        results.Should().HaveCount(2);
        results.Should().AllSatisfy(p => p.SeasonId.Should().Be(data.SeasonId));
    }

    [Fact]
    public async Task GetBySeasonAsync_ReturnsEmpty_ForWrongYear()
    {
        var data = await _seed.CreateFullSeedAsync("plt-wrongyear");
        await _repo.CreateAsync(MakePlanting(data));

        var results = await _repo.GetBySeasonAsync(data.GardenId, 2099);

        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetBySeasonAsync_FiltersByBedId()
    {
        var data = await _seed.CreateFullSeedAsync("plt-bedfilter");
        var otherBedId = await _seed.CreateBedAsync(data.GardenId, "Bed 2");

        var planting1 = MakePlanting(data);
        var planting2 = MakePlanting(data);
        planting2.BedId = otherBedId;
        await _repo.CreateAsync(planting1);
        await _repo.CreateAsync(planting2);

        var results = await _repo.GetBySeasonAsync(data.GardenId, 2025, bedId: data.BedId);

        results.Should().ContainSingle()
            .Which.BedId.Should().Be(data.BedId);
    }

    [Fact]
    public async Task GetBySeasonAsync_FiltersByPlantTypeId()
    {
        var data = await _seed.CreateFullSeedAsync("plt-typefilt");
        var otherTypeId = await _seed.CreatePlantTypeAsync("Pepper-plt-typefilt");
        var otherVarietyId = await _seed.CreatePlantVarietyAsync(otherTypeId, "Bell-plt-typefilt");

        var tomato = MakePlanting(data);
        var pepper = MakePlanting(data);
        pepper.PlantVarietyId = otherVarietyId;
        await _repo.CreateAsync(tomato);
        await _repo.CreateAsync(pepper);

        var results = await _repo.GetBySeasonAsync(data.GardenId, 2025, plantTypeId: data.PlantTypeId);

        results.Should().ContainSingle()
            .Which.PlantVarietyId.Should().Be(data.VarietyId);
    }

    [Fact]
    public async Task GetBySeasonAsync_PopulatesJoinedNames()
    {
        var data = await _seed.CreateFullSeedAsync("plt-joinnames");
        await _repo.CreateAsync(MakePlanting(data));

        var results = await _repo.GetBySeasonAsync(data.GardenId, 2025);

        var p = results.Should().ContainSingle().Subject;
        p.BedName.Should().Be("Bed 1");
        p.PlantVarietyName.Should().Be("Cherokee Purple-plt-joinnames");
        p.PlantTypeName.Should().Be("Tomato-plt-joinnames");
    }

    [Fact]
    public async Task GetBySeasonAsync_WithBulkPlantings_ReturnsAll()
    {
        var data = await _seed.CreateFullSeedAsync("plt-bulk");
        await _seed.CreatePlantingsAsync(data, 8);

        var results = await _repo.GetBySeasonAsync(data.GardenId, 2025);

        results.Should().HaveCount(8);
    }

    // ── Inventory link ──────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_PersistsInventoryLink()
    {
        var data = await _seed.CreateFullSeedAsync("plt-invlink");
        var inventoryItem = MakeInventoryItem(data);
        inventoryItem.Id = await _inventoryRepo.CreateAsync(inventoryItem);

        var planting = MakePlanting(data, inventoryItemId: inventoryItem.Id, qtyFromInventory: 10);
        planting.Id = await _repo.CreateAsync(planting);

        var fetched = await _repo.GetByIdAsync(planting.Id);
        fetched!.InventoryItemId.Should().Be(inventoryItem.Id);
        fetched.QuantityUsedFromInventory.Should().Be(10);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesInventoryLinkFields()
    {
        var data = await _seed.CreateFullSeedAsync("plt-invupdate");
        var inventoryItem = MakeInventoryItem(data);
        inventoryItem.Id = await _inventoryRepo.CreateAsync(inventoryItem);

        var planting = MakePlanting(data);
        planting.Id = await _repo.CreateAsync(planting);

        planting.InventoryItemId = inventoryItem.Id;
        planting.QuantityUsedFromInventory = 12;
        await _repo.UpdateAsync(planting);

        var fetched = await _repo.GetByIdAsync(planting.Id);
        fetched!.InventoryItemId.Should().Be(inventoryItem.Id);
        fetched.QuantityUsedFromInventory.Should().Be(12);
    }

    // ── UpdateAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_PersistsAllFields()
    {
        var data = await _seed.CreateFullSeedAsync("plt-update");
        var planting = MakePlanting(data);
        planting.Id = await _repo.CreateAsync(planting);

        planting.StartMethod = StartMethod.Transplant;
        planting.Quantity = 12;
        planting.TotalCost = 7.50m;
        planting.Notes = "Updated notes";
        await _repo.UpdateAsync(planting);

        var fetched = await _repo.GetByIdAsync(planting.Id);
        fetched!.StartMethod.Should().Be(StartMethod.Transplant);
        fetched.Quantity.Should().Be(12);
        fetched.TotalCost.Should().Be(7.50m);
        fetched.Notes.Should().Be("Updated notes");
    }

    // ── DeleteAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_RemovesPlanting()
    {
        var data = await _seed.CreateFullSeedAsync("plt-delete");
        var planting = MakePlanting(data);
        planting.Id = await _repo.CreateAsync(planting);

        await _repo.DeleteAsync(planting.Id);

        var fetched = await _repo.GetByIdAsync(planting.Id);
        fetched.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_DoesNotAffectOtherPlantings()
    {
        var data = await _seed.CreateFullSeedAsync("plt-delete-isolation");
        var p1 = MakePlanting(data);
        var p2 = MakePlanting(data);
        p1.Id = await _repo.CreateAsync(p1);
        p2.Id = await _repo.CreateAsync(p2);

        await _repo.DeleteAsync(p1.Id);

        var fetched = await _repo.GetByIdAsync(p2.Id);
        fetched.Should().NotBeNull();
    }

    [Fact]
    public async Task DeletePlanting_WithInventoryLink_DoesNotDeleteInventoryItem()
    {
        var data = await _seed.CreateFullSeedAsync("plt-inv-fk");
        var inventoryItem = MakeInventoryItem(data);
        inventoryItem.Id = await _inventoryRepo.CreateAsync(inventoryItem);

        var planting = MakePlanting(data, inventoryItemId: inventoryItem.Id, qtyFromInventory: 10);
        planting.Id = await _repo.CreateAsync(planting);

        // Deleting the planting should SET NULL on the FK, not cascade-delete the inventory item
        await _repo.DeleteAsync(planting.Id);

        var inv = await _inventoryRepo.GetByIdAsync(inventoryItem.Id);
        inv.Should().NotBeNull("inventory item must survive planting deletion");
    }
}
