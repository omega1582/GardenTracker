using GardenTracker.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GardenTracker.Data.Configurations;

public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Description).IsRequired().HasMaxLength(500);
        builder.Property(e => e.Amount).HasPrecision(10, 2);
        builder.Property(e => e.Category).IsRequired();
        builder.Property(e => e.ExpenseDate).IsRequired();

        builder.HasOne(e => e.Season)
            .WithMany(s => s.Expenses)
            .HasForeignKey(e => e.SeasonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Bed)
            .WithMany(b => b.Expenses)
            .HasForeignKey(e => e.BedId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.Supplier)
            .WithMany(s => s.Expenses)
            .HasForeignKey(e => e.SupplierId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
