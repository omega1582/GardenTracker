using FluentAssertions;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Interfaces.Repositories;
using GardenTracker.Services;
using Moq;

namespace GardenTracker.Tests.Services;

public class WaterBillServiceTests
{
    private readonly Mock<IWaterBillRepository> _waterBillRepo = new();
    private readonly WaterBillService _sut;

    public WaterBillServiceTests() => _sut = new WaterBillService(_waterBillRepo.Object);

    [Fact]
    public async Task GetByIdAsync_ReturnsBill_WhenUserOwnsIt()
    {
        var bill = new WaterBill { Id = 1, UserId = 42, Year = 2025, Month = 6 };
        _waterBillRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(bill);

        var result = await _sut.GetByIdAsync(1, userId: 42);

        result.Should().NotBeNull();
        result!.Month.Should().Be(6);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenUserDoesNotOwnBill()
    {
        var bill = new WaterBill { Id = 1, UserId = 42 };
        _waterBillRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(bill);

        var result = await _sut.GetByIdAsync(1, userId: 99);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenBillDoesNotExist()
    {
        _waterBillRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((WaterBill?)null);

        var result = await _sut.GetByIdAsync(1, userId: 42);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_SetUserIdAndReturnsCreatedBill_WhenNoExistingBillForMonth()
    {
        _waterBillRepo.Setup(r => r.GetByYearMonthAsync(42, 2025, 6)).ReturnsAsync((WaterBill?)null);
        _waterBillRepo.Setup(r => r.CreateAsync(It.IsAny<WaterBill>())).ReturnsAsync(10);

        var bill = new WaterBill { Year = 2025, Month = 6, TotalCost = 55.00m };
        var result = await _sut.CreateAsync(userId: 42, bill);

        result.Should().NotBeNull();
        result!.UserId.Should().Be(42);
        result.Id.Should().Be(10);
    }

    [Fact]
    public async Task CreateAsync_ReturnsNull_WhenBillAlreadyExistsForMonth()
    {
        var existing = new WaterBill { Id = 5, UserId = 42, Year = 2025, Month = 6 };
        _waterBillRepo.Setup(r => r.GetByYearMonthAsync(42, 2025, 6)).ReturnsAsync(existing);

        var bill = new WaterBill { Year = 2025, Month = 6, TotalCost = 55.00m };
        var result = await _sut.CreateAsync(userId: 42, bill);

        result.Should().BeNull();
        _waterBillRepo.Verify(r => r.CreateAsync(It.IsAny<WaterBill>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsTrue_AndUpdatesFields_WhenUserOwnsBill()
    {
        var bill = new WaterBill { Id = 1, UserId = 42, UsageCcf = 1.0m, UsageGallons = 748m, TotalCost = 45.00m, IsGardenActive = false };
        _waterBillRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(bill);

        var updated = new WaterBill { UsageCcf = 2.5m, UsageGallons = 1870m, TotalCost = 72.50m, IsGardenActive = true, Notes = "Hot month" };
        var result = await _sut.UpdateAsync(1, userId: 42, updated);

        result.Should().BeTrue();
        _waterBillRepo.Verify(r => r.UpdateAsync(It.Is<WaterBill>(b =>
            b.UsageCcf == 2.5m &&
            b.UsageGallons == 1870m &&
            b.TotalCost == 72.50m &&
            b.IsGardenActive == true &&
            b.Notes == "Hot month")), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenUserDoesNotOwnBill()
    {
        var bill = new WaterBill { Id = 1, UserId = 42 };
        _waterBillRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(bill);

        var result = await _sut.UpdateAsync(1, userId: 99, new WaterBill());

        result.Should().BeFalse();
        _waterBillRepo.Verify(r => r.UpdateAsync(It.IsAny<WaterBill>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsTrue_WhenUserOwnsBill()
    {
        var bill = new WaterBill { Id = 1, UserId = 42 };
        _waterBillRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(bill);

        var result = await _sut.DeleteAsync(1, userId: 42);

        result.Should().BeTrue();
        _waterBillRepo.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenUserDoesNotOwnBill()
    {
        var bill = new WaterBill { Id = 1, UserId = 42 };
        _waterBillRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(bill);

        var result = await _sut.DeleteAsync(1, userId: 99);

        result.Should().BeFalse();
        _waterBillRepo.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
    }
}
