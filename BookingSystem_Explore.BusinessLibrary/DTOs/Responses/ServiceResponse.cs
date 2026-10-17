using System;

namespace BookingSystem_Explore.BusinessLibrary.DTOs.Responses
{
    public class ServiceResponse
    {
        public Guid ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
    }
}
