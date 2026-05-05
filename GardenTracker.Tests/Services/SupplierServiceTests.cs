using FluentAssertions;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Enums;
using GardenTracker.Core.Interfaces.Repositories;
using GardenTracker.Services;
using Moq;

namespace GardenTracker.Tests.Services;

public class SupplierServiceTests
{
    private readonly Mock<ISupplierRepository> _repo = new();
    private readonly SupplierService _sut;

    public SupplierServiceTests() => _sut = new SupplierService(_repo.Object);

    [Fact]
    public async Task CreateAsync_SetsAllFields()
    {
        _repo.Setup(r => r.CreateAsync(It.IsAny<Supplier>())).ReturnsAsync(1);

        var result = await _sut.CreateAsync("Baker Creek", SupplierType.Online, "rareseeds.com", "Great heirlooms");

        result.Name.Should().Be("Baker Creek");
        result.SupplierType.Should().Be(SupplierType.Online);
        result.Website.Should().Be("rareseeds.com");
        result.Notes.Should().Be("Great heirlooms");
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenSupplierNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Supplier?)null);

        var result = await _sut.UpdateAsync(99, "Name", SupplierType.Other, null, null);

        result.Should().BeFalse();
        _repo.Verify(r => r.UpdateAsync(It.IsAny<Supplier>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsTrue_AndUpdatesFields()
    {
        var supplier = new Supplier { Id = 1, Name = "Old", SupplierType = SupplierType.BigBox };
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(supplier);

        var result = await _sut.UpdateAsync(1, "New Name", SupplierType.LocalNursery, "site.com", null);

        result.Should().BeTrue();
        _repo.Verify(r => r.UpdateAsync(It.Is<Supplier>(s =>
            s.Name == "New Name" && s.SupplierType == SupplierType.LocalNursery)), Times.Once);
    }
}
