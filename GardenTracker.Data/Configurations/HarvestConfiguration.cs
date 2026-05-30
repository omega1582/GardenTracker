using GardenTracker.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GardenTracker.Data.Configurations;

public class HarvestConfiguration : IEntityTypeConfiguration<Harvest>
{
    public void Configure(EntityTypeBuilder<Harvest> builder)
    {
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Quantity).HasPrecision(10, 3);
        builder.Property(h => h.Unit).IsRequired();
        builder.Property(h => h.HarvestDate).IsRequired();

        builder.HasOne(h => h.Bed)
            .WithMany(b => b.Harvests)
            .HasForeignKey(h => h.BedId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(h => h.Season)
            .WithMany(s => s.Harvests)
            .HasForeignKey(h => h.SeasonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(h => h.PlantVariety)
            .WithMany(v => v.Harvests)
            .HasForeignKey(h => h.PlantVarietyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(h => h.BedName);
        builder.Ignore(h => h.PlantVarietyName);
        builder.Ignore(h => h.PlantTypeName);
    }
}
