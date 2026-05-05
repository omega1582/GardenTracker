using GardenTracker.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GardenTracker.Data.Configurations;

public class PlantTypeConfiguration : IEntityTypeConfiguration<PlantType>
{
    public void Configure(EntityTypeBuilder<PlantType> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
        builder.HasIndex(p => p.Name).IsUnique();
    }
}
