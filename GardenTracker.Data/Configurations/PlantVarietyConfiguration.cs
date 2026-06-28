using GardenTracker.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GardenTracker.Data.Configurations;

public class PlantVarietyConfiguration : IEntityTypeConfiguration<PlantVariety>
{
    public void Configure(EntityTypeBuilder<PlantVariety> builder)
    {
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Name).IsRequired().HasMaxLength(150);
        builder.HasIndex(v => new { v.PlantTypeId, v.Name }).IsUnique();
        builder.Property(v => v.GrowthHabit).HasConversion<string>();
        builder.Property(v => v.SunPreference).HasConversion<string>();
        builder.Property(v => v.ImageUrl).HasMaxLength(500);

        builder.HasOne(v => v.PlantType)
            .WithMany(p => p.Varieties)
            .HasForeignKey(v => v.PlantTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
