using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BookingSystem_Explore.Domain.Entities;

namespace BookingSystem_Explore.Infrastructure.Persistence.Configurations
{
    public class AppointmentSlotConfiguration : IEntityTypeConfiguration<AppointmentSlot>
    {
        public void Configure(EntityTypeBuilder<AppointmentSlot> builder)
        {
            builder.ToTable("AppointmentSlots");
            builder.HasKey(s => s.AppointmentSlotId);
            builder.Property(s => s.StartDateTimeUtc).IsRequired();
            builder.Property(s => s.EndDateTimeUtc).IsRequired();
            builder.Property(s => s.IsActive).HasDefaultValue(true);
            builder.Property(s => s.CreatedDate).IsRequired();

            builder.HasIndex(s => new { s.BranchServiceId, s.StartDateTimeUtc }).IsUnique();

            builder.HasOne(s => s.BranchService).WithMany(bs => bs.AppointmentSlots).HasForeignKey(s => s.BranchServiceId).OnDelete(DeleteBehavior.Restrict);

            // constraint: EndDateTimeUtc > StartDateTimeUtc
            builder.HasCheckConstraint("CK_AppointmentSlot_TimeRange", "EndDateTimeUtc > StartDateTimeUtc");
        }
    }
}
