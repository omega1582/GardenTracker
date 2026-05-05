using GardenTracker.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GardenTracker.Data.Configurations;

public class GardenConfiguration : IEntityTypeConfiguration<Garden>
{
    public void Configure(EntityTypeBuilder<Garden> builder)
    {
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Name).IsRequired().HasMaxLength(150);
        builder.Property(g => g.Location).HasMaxLength(250);
        builder.Property(g => g.CreatedAt).IsRequired();

        builder.HasOne(g => g.User)
            .WithMany(u => u.Gardens)
            .HasForeignKey(g => g.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
