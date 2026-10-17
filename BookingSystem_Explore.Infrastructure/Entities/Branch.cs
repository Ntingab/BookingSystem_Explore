using System;
using System.Collections.Generic;

namespace BookingSystem_Explore.Infrastructure.Entities
{
    public class Branch
    {
        public Guid BranchId { get; set; }
        public string BranchCode { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string TimeZoneId { get; set; } = string.Empty;
        public string? ContactNumber { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public ICollection<BranchService>? BranchServices { get; set; }
    }
}
