namespace BookingSystem_Explore.BusinessLibrary.DTOs.Responses
{
    public class BranchServiceResponse
    {
        public System.Guid BranchServiceId { get; set; }
        public System.Guid BranchId { get; set; }
        public System.Guid ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
    }
}
