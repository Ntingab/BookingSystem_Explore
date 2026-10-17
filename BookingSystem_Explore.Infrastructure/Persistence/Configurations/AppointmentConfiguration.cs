using BookingSystem_Explore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingSystem_Explore.Infrastructure.Persistence.Configurations
{
    public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
    {
        public void Configure(EntityTypeBuilder<Appointment> builder)
        {
            builder.ToTable("Appointments");
            builder.HasKey(a => a.AppointmentId);
            builder.Property(a => a.AppointmentReference).IsRequired().HasMaxLength(30);
            builder.Property(a => a.Status).IsRequired();
            builder.Property(a => a.Notes).HasMaxLength(500);
            builder.Property(a => a.CreatedDate).IsRequired();
            builder.Property(a => a.UpdatedDate);
            builder.Property(a => a.CancelledDate);
            builder.Property(a => a.ManagementTokenHash).IsRequired().HasMaxLength(256);

            builder.HasIndex(a => a.AppointmentReference).IsUnique();

            // Filtered unique index to prevent multiple confirmed/completed/no-show for same slot
            // EF Core does not have direct API for filtered unique indexes portable across providers; use raw SQL if necessary.
            builder.HasIndex(a => a.AppointmentSlotId).HasDatabaseName("IX_Appointments_AppointmentSlotId_Status_Filtered");

            builder.HasOne(a => a.Customer).WithMany(c => c.Appointments).HasForeignKey(a => a.CustomerId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(a => a.AppointmentSlot).WithMany(s => s.Appointments).HasForeignKey(a => a.AppointmentSlotId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
