using FluentAssertions;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Enums;
using GardenTracker.Core.Interfaces.Repositories;
using GardenTracker.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace GardenTracker.Tests.Services;

public class InventoryServiceTests
{
    private readonly Mock<IInventoryRepository> _repo = new();
    private readonly InventoryService _sut;

    public InventoryServiceTests() =>
        _sut = new InventoryService(_repo.Object, NullLogger<InventoryService>.Instance);

    private InventoryItem MakeItem(int userId = 42, int purchased = 50, int remaining = 40) => new()
    {
        Id = 1,
        UserId = userId,
        PlantVarietyId = 10,
        Type = InventoryType.Seed,
        QuantityPurchased = purchased,
        QuantityRemaining = remaining,
        TotalCost = 3.99m,
        PurchaseDate = new DateOnly(2025, 3, 1)
    };

    // ── GetByIdAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ReturnsItem_WhenUserOwnsIt()
    {
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(MakeItem(userId: 42));

        var result = await _sut.GetByIdAsync(1, userId: 42);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenItemNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((InventoryItem?)null);

        var result = await _sut.GetByIdAsync(99, userId: 42);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenUserDoesNotOwnItem()
    {
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(MakeItem(userId: 42));

        var result = await _sut.GetByIdAsync(1, userId: 99);

        result.Should().BeNull();
    }

    // ── CreateAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_SetsUserId()
    {
        _repo.Setup(r => r.CreateAsync(It.IsAny<InventoryItem>())).ReturnsAsync(7);
        var item = MakeItem(userId: 0);

        await _sut.CreateAsync(userId: 42, item);

        item.UserId.Should().Be(42);
    }

    [Fact]
    public async Task CreateAsync_SetsQuantityRemainingEqualToPurchased()
    {
        _repo.Setup(r => r.CreateAsync(It.IsAny<InventoryItem>())).ReturnsAsync(7);
        var item = new InventoryItem { QuantityPurchased = 50, QuantityRemaining = 0 };

        await _sut.CreateAsync(userId: 42, item);

        item.QuantityRemaining.Should().Be(50);
    }

    [Fact]
    public async Task CreateAsync_ReturnsItemWithAssignedId()
    {
        _repo.Setup(r => r.CreateAsync(It.IsAny<InventoryItem>())).ReturnsAsync(7);
        var item = MakeItem();

        var result = await _sut.CreateAsync(userId: 42, item);

        result.Id.Should().Be(7);
    }

    // ── UpdateAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenItemNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((InventoryItem?)null);

        var result = await _sut.UpdateAsync(99, userId: 42, null, 50, 3.99m, new DateOnly(2025, 1, 1), null);

        result.Should().BeFalse();
        _repo.Verify(r => r.UpdateAsync(It.IsAny<InventoryItem>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenUserDoesNotOwnItem()
    {
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(MakeItem(userId: 42));

        var result = await _sut.UpdateAsync(1, userId: 99, null, 50, 3.99m, new DateOnly(2025, 1, 1), null);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_IncreasesRemaining_WhenPurchasedQuantityIncreases()
    {
        // purchased=50, remaining=40, new purchased=60 → delta=+10 → remaining=50
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(MakeItem(purchased: 50, remaining: 40));

        await _sut.UpdateAsync(1, userId: 42, null, quantityPurchased: 60, 3.99m, new DateOnly(2025, 1, 1), null);

        _repo.Verify(r => r.UpdateRemainingQuantityAsync(1, 50), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_DecreasesRemaining_WhenPurchasedQuantityDecreases()
    {
        // purchased=50, remaining=40, new purchased=40 → delta=-10 → remaining=30
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(MakeItem(purchased: 50, remaining: 40));

        await _sut.UpdateAsync(1, userId: 42, null, quantityPurchased: 40, 3.99m, new DateOnly(2025, 1, 1), null);

        _repo.Verify(r => r.UpdateRemainingQuantityAsync(1, 30), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ClampsRemainingToZero_WhenDeltaWouldGoNegative()
    {
        // purchased=50, remaining=5, new purchased=30 → delta=-20 → remaining would be -15, clamped to 0
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(MakeItem(purchased: 50, remaining: 5));

        await _sut.UpdateAsync(1, userId: 42, null, quantityPurchased: 30, 3.99m, new DateOnly(2025, 1, 1), null);

        _repo.Verify(r => r.UpdateRemainingQuantityAsync(1, 0), Times.Once);
    }

    // ── AdjustRemainingAsync ────────────────────────────────────────────────

    [Fact]
    public async Task AdjustRemainingAsync_ReturnsFalse_WhenUserDoesNotOwnItem()
    {
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(MakeItem(userId: 42));

        var result = await _sut.AdjustRemainingAsync(1, userId: 99, newRemaining: 10);

        result.Should().BeFalse();
        _repo.Verify(r => r.UpdateRemainingQuantityAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task AdjustRemainingAsync_ReturnsFalse_WhenItemNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((InventoryItem?)null);

        var result = await _sut.AdjustRemainingAsync(99, userId: 42, newRemaining: 10);

        result.Should().BeFalse();
        _repo.Verify(r => r.UpdateRemainingQuantityAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task AdjustRemainingAsync_UpdatesRemaining_WhenOwned()
    {
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(MakeItem(userId: 42));

        var result = await _sut.AdjustRemainingAsync(1, userId: 42, newRemaining: 15);

        result.Should().BeTrue();
        _repo.Verify(r => r.UpdateRemainingQuantityAsync(1, 15), Times.Once);
    }

    // ── DeleteAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenItemNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((InventoryItem?)null);

        var result = await _sut.DeleteAsync(99, userId: 42);

        result.Should().BeFalse();
        _repo.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_DeletesItem_WhenOwned()
    {
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(MakeItem(userId: 42));

        var result = await _sut.DeleteAsync(1, userId: 42);

        result.Should().BeTrue();
        _repo.Verify(r => r.DeleteAsync(1), Times.Once);
    }
}
