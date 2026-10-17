using System;
using System.Collections.Generic;

namespace BookingSystem_Explore.Infrastructure.Entities
{
    public class AppointmentSlot
    {
        public Guid AppointmentSlotId { get; set; }
        public Guid BranchServiceId { get; set; }
        public DateTime StartDateTimeUtc { get; set; }
        public DateTime EndDateTimeUtc { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; }

        public BranchService? BranchService { get; set; }
        public ICollection<Appointment>? Appointments { get; set; }
    }
}
