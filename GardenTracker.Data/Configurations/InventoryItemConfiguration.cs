using GardenTracker.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GardenTracker.Data.Configurations;

public class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.TotalCost).HasPrecision(10, 2);
        builder.Property(i => i.Type).IsRequired();
        builder.Property(i => i.PurchaseDate).IsRequired();

        builder.Ignore(i => i.PlantVarietyName);
        builder.Ignore(i => i.PlantTypeName);
        builder.Ignore(i => i.SupplierName);

        builder.HasOne(i => i.User)
            .WithMany()
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.PlantVariety)
            .WithMany()
            .HasForeignKey(i => i.PlantVarietyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Supplier)
            .WithMany()
            .HasForeignKey(i => i.SupplierId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
