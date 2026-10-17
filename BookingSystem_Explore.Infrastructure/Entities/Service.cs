using System;
using System.Collections.Generic;

namespace BookingSystem_Explore.Infrastructure.Entities
{
    public class Service
    {
        public Guid ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DurationMinutes { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public ICollection<BranchService>? BranchServices { get; set; }
    }
}
