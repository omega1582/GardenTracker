using GardenTracker.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GardenTracker.Data.Configurations;

public class SeasonConfiguration : IEntityTypeConfiguration<Season>
{
    public void Configure(EntityTypeBuilder<Season> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Year).IsRequired();
        builder.HasIndex(s => new { s.GardenId, s.Year }).IsUnique();

        builder.HasOne(s => s.Garden)
            .WithMany(g => g.Seasons)
            .HasForeignKey(s => s.GardenId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
