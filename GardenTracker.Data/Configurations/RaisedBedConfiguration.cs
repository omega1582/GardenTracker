using GardenTracker.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GardenTracker.Data.Configurations;

public class BedConfiguration : IEntityTypeConfiguration<Bed>
{
    public void Configure(EntityTypeBuilder<Bed> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Name).IsRequired().HasMaxLength(100);
        builder.Property(b => b.LengthFt).HasPrecision(6, 2);
        builder.Property(b => b.WidthFt).HasPrecision(6, 2);
        builder.Property(b => b.DepthIn).HasPrecision(6, 2);
        builder.Property(b => b.Material).HasMaxLength(200);
        builder.Property(b => b.InstalledDate).IsRequired();

        builder.HasOne(b => b.Garden)
            .WithMany(g => g.Beds)
            .HasForeignKey(b => b.GardenId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
