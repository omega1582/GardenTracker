using GardenTracker.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GardenTracker.Data.Configurations;

public class BedPlantingConfiguration : IEntityTypeConfiguration<BedPlanting>
{
    public void Configure(EntityTypeBuilder<BedPlanting> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.TotalCost).HasPrecision(10, 2);
        builder.Property(p => p.StartMethod).IsRequired();

        builder.HasOne(p => p.Bed)
            .WithMany(b => b.Plantings)
            .HasForeignKey(p => p.BedId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Season)
            .WithMany(s => s.Plantings)
            .HasForeignKey(p => p.SeasonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.PlantVariety)
            .WithMany(v => v.Plantings)
            .HasForeignKey(p => p.PlantVarietyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Supplier)
            .WithMany(s => s.Plantings)
            .HasForeignKey(p => p.SupplierId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.SourceHarvest)
            .WithMany(h => h.SeedSavedPlantings)
            .HasForeignKey(p => p.SourceHarvestId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
