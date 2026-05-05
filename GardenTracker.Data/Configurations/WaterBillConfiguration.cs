using GardenTracker.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GardenTracker.Data.Configurations;

public class WaterBillConfiguration : IEntityTypeConfiguration<WaterBill>
{
    public void Configure(EntityTypeBuilder<WaterBill> builder)
    {
        builder.HasKey(w => w.Id);
        builder.Property(w => w.UsageCcf).HasPrecision(10, 4);
        builder.Property(w => w.UsageGallons).HasPrecision(10, 2);
        builder.Property(w => w.TotalCost).HasPrecision(10, 2);
        builder.Property(w => w.Notes).HasMaxLength(500);

        builder.HasIndex(w => new { w.UserId, w.Year, w.Month }).IsUnique();

        builder.HasOne(w => w.User)
            .WithMany()
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
