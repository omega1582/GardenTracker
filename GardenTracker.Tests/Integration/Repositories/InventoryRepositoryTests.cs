using FluentAssertions;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Enums;
using GardenTracker.Data.Repositories;

namespace GardenTracker.Tests.Integration.Repositories;

[Trait("Category", "Integration")]
public class InventoryRepositoryTests(DatabaseFixture db) : IClassFixture<DatabaseFixture>
{
    private readonly InventoryRepository _repo = new(db.ConnectionFactory);
    private readonly TestDataBuilder _seed = new(db.ConnectionFactory);

    private InventoryItem MakeItem(int userId, int varietyId, int qty = 50) => new()
    {
        UserId = userId,
        PlantVarietyId = varietyId,
        Type = InventoryType.Seed,
        QuantityPurchased = qty,
        QuantityRemaining = qty,
        TotalCost = 3.99m,
        PurchaseDate = new DateOnly(2025, 3, 15),
        Notes = "Test packet"
    };

    [Fact]
    public async Task CreateAsync_InsertsItem_AndReturnsGeneratedId()
    {
        var data = await _seed.CreateFullSeedAsync("inv-create");
        var item = MakeItem(data.UserId, data.VarietyId);

        var id = await _repo.CreateAsync(item);

        id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsPersistData()
    {
        var data = await _seed.CreateFullSeedAsync("inv-getbyid");
        var item = MakeItem(data.UserId, data.VarietyId, qty: 30);
        item.Id = await _repo.CreateAsync(item);

        var fetched = await _repo.GetByIdAsync(item.Id);

        fetched.Should().NotBeNull();
        fetched!.QuantityPurchased.Should().Be(30);
        fetched.QuantityRemaining.Should().Be(30);
        fetched.TotalCost.Should().Be(3.99m);
        fetched.PurchaseDate.Should().Be(new DateOnly(2025, 3, 15));
        fetched.Notes.Should().Be("Test packet");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_ForMissingId()
    {
        var result = await _repo.GetByIdAsync(999999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByUserAsync_ReturnsOnlyThatUsersItems()
    {
        var dataA = await _seed.CreateFullSeedAsync("inv-user-a");
        var dataB = await _seed.CreateFullSeedAsync("inv-user-b");
        await _repo.CreateAsync(MakeItem(dataA.UserId, dataA.VarietyId));
        await _repo.CreateAsync(MakeItem(dataA.UserId, dataA.VarietyId));
        await _repo.CreateAsync(MakeItem(dataB.UserId, dataB.VarietyId));

        var results = await _repo.GetByUserAsync(dataA.UserId);

        results.Should().HaveCount(2);
        results.Should().AllSatisfy(i => i.UserId.Should().Be(dataA.UserId));
    }

    [Fact]
    public async Task GetByUserAsync_PopulatesJoinedNames()
    {
        var data = await _seed.CreateFullSeedAsync("inv-names");
        await _repo.CreateAsync(MakeItem(data.UserId, data.VarietyId));

        var results = await _repo.GetByUserAsync(data.UserId);

        var item = results.Should().ContainSingle().Subject;
        item.PlantVarietyName.Should().Be("Cherokee Purple-inv-names");
        item.PlantTypeName.Should().Be("Tomato-inv-names");
    }

    [Fact]
    public async Task GetByVarietyAsync_ReturnsItemsForVariety()
    {
        var data = await _seed.CreateFullSeedAsync("inv-variety");
        var otherVarietyId = await _seed.CreatePlantVarietyAsync(data.PlantTypeId, "Roma");
        await _repo.CreateAsync(MakeItem(data.UserId, data.VarietyId));
        await _repo.CreateAsync(MakeItem(data.UserId, otherVarietyId));

        var results = await _repo.GetByVarietyAsync(data.VarietyId, data.UserId);

        results.Should().ContainSingle()
            .Which.PlantVarietyId.Should().Be(data.VarietyId);
    }

    [Fact]
    public async Task UpdateRemainingQuantityAsync_ChangesOnlyRemainingField()
    {
        var data = await _seed.CreateFullSeedAsync("inv-adjust");
        var item = MakeItem(data.UserId, data.VarietyId, qty: 50);
        item.Id = await _repo.CreateAsync(item);

        await _repo.UpdateRemainingQuantityAsync(item.Id, 38);

        var fetched = await _repo.GetByIdAsync(item.Id);
        fetched!.QuantityRemaining.Should().Be(38);
        fetched.QuantityPurchased.Should().Be(50); // unchanged
    }

    [Fact]
    public async Task UpdateAsync_PersistsChangedFields()
    {
        var data = await _seed.CreateFullSeedAsync("inv-update");
        var item = MakeItem(data.UserId, data.VarietyId, qty: 50);
        item.Id = await _repo.CreateAsync(item);

        item.QuantityPurchased = 60;
        item.TotalCost = 5.49m;
        item.Notes = "Updated notes";
        await _repo.UpdateAsync(item);

        var fetched = await _repo.GetByIdAsync(item.Id);
        fetched!.QuantityPurchased.Should().Be(60);
        fetched.TotalCost.Should().Be(5.49m);
        fetched.Notes.Should().Be("Updated notes");
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        var data = await _seed.CreateFullSeedAsync("inv-delete");
        var item = MakeItem(data.UserId, data.VarietyId);
        item.Id = await _repo.CreateAsync(item);

        await _repo.DeleteAsync(item.Id);

        var fetched = await _repo.GetByIdAsync(item.Id);
        fetched.Should().BeNull();
    }
}
