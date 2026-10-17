using System;
using System.Collections.Generic;

namespace BookingSystem_Explore.Infrastructure.Entities
{
    public class BranchService
    {
        public Guid BranchServiceId { get; set; }
        public Guid BranchId { get; set; }
        public Guid ServiceId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; }

        public Branch? Branch { get; set; }
        public Service? Service { get; set; }
        public ICollection<AppointmentSlot>? AppointmentSlots { get; set; }
    }
}
