using System;

namespace BookingSystem_Explore.BusinessLibrary.DTOs.Requests
{
    public class CreateBranchServiceRequest
    {
        public Guid ServiceId { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
