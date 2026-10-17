using System;
using BookingSystem_Explore.Domain.Enums;

namespace BookingSystem_Explore.Domain.Entities
{
    public class Notification
    {
        public Guid NotificationId { get; set; }
        public Guid AppointmentId { get; set; }
        public NotificationType NotificationType { get; set; }
        public NotificationChannel Channel { get; set; }
        public string Recipient { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? SentDate { get; set; }
        public string? ErrorMessage { get; set; }

        public Appointment? Appointment { get; set; }
    }
}
