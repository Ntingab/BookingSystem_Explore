using System;

namespace BookingSystem_Explore.BusinessLibrary.DTOs.Requests
{
    public class CreateAppointmentSlotRequest
    {
        public Guid BranchServiceId { get; set; }
        public DateTime StartDateTimeUtc { get; set; }
        public DateTime EndDateTimeUtc { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
