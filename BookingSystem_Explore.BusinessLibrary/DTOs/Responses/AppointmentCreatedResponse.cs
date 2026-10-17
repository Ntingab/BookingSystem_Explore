using System;

namespace BookingSystem_Explore.BusinessLibrary.DTOs.Responses
{
    public class AppointmentCreatedResponse
    {
        public Guid AppointmentId { get; set; }
        public string AppointmentReference { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public DateTime StartDateTimeUtc { get; set; }
        public DateTime EndDateTimeUtc { get; set; }
        public string NotificationStatus { get; set; } = string.Empty;
        // Management token returned only at creation
        public string ManagementToken { get; set; } = string.Empty;
    }
}
