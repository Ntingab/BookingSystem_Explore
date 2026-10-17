using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BookingSystem_Explore.Domain.Entities;

namespace BookingSystem_Explore.Infrastructure.Persistence.Configurations
{
    public class ServiceConfiguration : IEntityTypeConfiguration<Service>
    {
        public void Configure(EntityTypeBuilder<Service> builder)
        {
            builder.ToTable("Services");
            builder.HasKey(s => s.ServiceId);
            builder.Property(s => s.ServiceName).IsRequired().HasMaxLength(150);
            builder.Property(s => s.Description).HasMaxLength(500);
            builder.Property(s => s.DurationMinutes).IsRequired();
            builder.Property(s => s.IsActive).HasDefaultValue(true);
            builder.Property(s => s.CreatedDate).IsRequired();
            builder.Property(s => s.UpdatedDate);

            builder.HasMany(s => s.BranchServices).WithOne(bs => bs.Service).HasForeignKey(bs => bs.ServiceId).OnDelete(DeleteBehavior.Restrict);

            // check constraint for positive duration
            builder.HasCheckConstraint("CK_Service_DurationPositive", "DurationMinutes > 0");
        }
    }
}
