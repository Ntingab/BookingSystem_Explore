using System;

namespace BookingSystem_Explore.BusinessLibrary.DTOs.Responses
{
    public class AppointmentSlotResponse
    {
        public Guid AppointmentSlotId { get; set; }
        public Guid BranchServiceId { get; set; }
        public DateTime StartDateTimeUtc { get; set; }
        public DateTime EndDateTimeUtc { get; set; }
        public bool IsActive { get; set; }
    }
}
