using System;

namespace BookingSystem_Explore.BusinessLibrary.DTOs.Requests
{
    public class UpdateAppointmentSlotRequest
    {
        public DateTime StartDateTimeUtc { get; set; }
        public DateTime EndDateTimeUtc { get; set; }
        public bool IsActive { get; set; }
    }
}
