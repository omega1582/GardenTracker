using GardenTracker.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GardenTracker.Data.Configurations;

public class MarketPriceConfiguration : IEntityTypeConfiguration<MarketPrice>
{
    public void Configure(EntityTypeBuilder<MarketPrice> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.PricePerUnit).HasPrecision(10, 2);
        builder.Property(m => m.Unit).IsRequired();
        builder.Property(m => m.RecordedDate).IsRequired();

        builder.HasOne(m => m.Season)
            .WithMany(s => s.MarketPrices)
            .HasForeignKey(m => m.SeasonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.PlantType)
            .WithMany(p => p.MarketPrices)
            .HasForeignKey(m => m.PlantTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.PlantVariety)
            .WithMany(v => v.MarketPrices)
            .HasForeignKey(m => m.PlantVarietyId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
