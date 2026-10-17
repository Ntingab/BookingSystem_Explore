using System;
using System.Collections.Generic;
using BookingSystem_Explore.Infrastructure.Enums;

namespace BookingSystem_Explore.Infrastructure.Entities
{
    public class Appointment
    {
        public Guid AppointmentId { get; set; }
        public string AppointmentReference { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
        public Guid AppointmentSlotId { get; set; }
        public AppointmentStatus Status { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public DateTime? CancelledDate { get; set; }

        public Customer? Customer { get; set; }
        public AppointmentSlot? AppointmentSlot { get; set; }
        public ICollection<Notification>? Notifications { get; set; }
    }
}
