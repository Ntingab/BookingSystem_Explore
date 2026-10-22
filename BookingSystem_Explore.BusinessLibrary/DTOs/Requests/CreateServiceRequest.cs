namespace BookingSystem_Explore.BusinessLibrary.DTOs.Requests
{
    public class CreateServiceRequest
    {
        public string ServiceName { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
