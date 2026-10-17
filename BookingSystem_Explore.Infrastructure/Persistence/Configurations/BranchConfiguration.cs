using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BookingSystem_Explore.Domain.Entities;

namespace BookingSystem_Explore.Infrastructure.Persistence.Configurations
{
    public class BranchConfiguration : IEntityTypeConfiguration<Branch>
    {
        public void Configure(EntityTypeBuilder<Branch> builder)
        {
            builder.ToTable("Branches");
            builder.HasKey(b => b.BranchId);
            builder.Property(b => b.BranchCode).IsRequired().HasMaxLength(20);
            builder.Property(b => b.BranchName).IsRequired().HasMaxLength(150);
            builder.Property(b => b.Address).IsRequired().HasMaxLength(250);
            builder.Property(b => b.City).IsRequired().HasMaxLength(100);
            builder.Property(b => b.Province).IsRequired().HasMaxLength(100);
            builder.Property(b => b.TimeZoneId).IsRequired().HasMaxLength(100);
            builder.Property(b => b.ContactNumber).HasMaxLength(20);
            builder.Property(b => b.IsActive).HasDefaultValue(true);
            builder.Property(b => b.CreatedDate).IsRequired();
            builder.Property(b => b.UpdatedDate);

            builder.HasIndex(b => b.BranchCode).IsUnique();

            builder.HasMany(b => b.BranchServices).WithOne(bs => bs.Branch).HasForeignKey(bs => bs.BranchId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
