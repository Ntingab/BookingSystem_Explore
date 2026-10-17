using System;

namespace BookingSystem_Explore.BusinessLibrary.DTOs.Requests
{
    public class CancelAppointmentRequest
    {
        public string ManagementToken { get; set; } = string.Empty;
    }
}
