using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BookingSystem_Explore.Domain.Entities;

namespace BookingSystem_Explore.Infrastructure.Persistence.Configurations
{
    public class BranchServiceConfiguration : IEntityTypeConfiguration<BranchService>
    {
        public void Configure(EntityTypeBuilder<BranchService> builder)
        {
            builder.ToTable("BranchServices");
            builder.HasKey(bs => bs.BranchServiceId);
            builder.Property(bs => bs.IsActive).HasDefaultValue(true);
            builder.Property(bs => bs.CreatedDate).IsRequired();

            builder.HasIndex(bs => new { bs.BranchId, bs.ServiceId }).IsUnique();

            builder.HasOne(bs => bs.Branch)
                .WithMany(b => b.BranchServices)
                .HasForeignKey(bs => bs.BranchId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(bs => bs.Service)
                .WithMany(s => s.BranchServices)
                .HasForeignKey(bs => bs.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
