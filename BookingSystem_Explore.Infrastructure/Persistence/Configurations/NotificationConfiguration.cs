using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BookingSystem_Explore.Domain.Entities;

namespace BookingSystem_Explore.Infrastructure.Persistence.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("Notifications");
            builder.HasKey(n => n.NotificationId);
            builder.Property(n => n.NotificationType).IsRequired();
            builder.Property(n => n.Channel).IsRequired();
            builder.Property(n => n.Recipient).IsRequired().HasMaxLength(254);
            builder.Property(n => n.Message).IsRequired().HasMaxLength(2000);
            builder.Property(n => n.Status).IsRequired();
            builder.Property(n => n.CreatedDate).IsRequired();
            builder.Property(n => n.SentDate);
            builder.Property(n => n.ErrorMessage).HasMaxLength(500);

            builder.HasOne(n => n.Appointment).WithMany(a => a.Notifications).HasForeignKey(n => n.AppointmentId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
