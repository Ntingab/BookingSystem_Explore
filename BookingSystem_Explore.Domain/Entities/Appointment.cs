using BookingSystem_Explore.Domain.Enums;

namespace BookingSystem_Explore.Domain.Entities
{
    public class Appointment
    {
        public Guid AppointmentId { get; set; }
        // kept property name matching DB config
        public string AppointmentReference { get; set; } = string.Empty;
        public Guid AppointmentSlotId { get; set; }
        public Guid CustomerId { get; set; }
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string? Notes { get; set; }
        // Persisted hash of the management token. Make non-nullable so DB column is required.
        public string ManagementTokenHash { get; set; } = string.Empty;
        public AppointmentSlot? AppointmentSlot { get; set; }
        public Customer? Customer { get; set; }
        public ICollection<Notification>? Notifications { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public DateTime? CancelledDate { get; set; }
    }
}
