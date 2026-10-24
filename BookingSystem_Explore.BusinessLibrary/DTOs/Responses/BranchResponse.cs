namespace BookingSystem_Explore.BusinessLibrary.DTOs.Responses
{
    public class BranchResponse
    {
        public System.Guid BranchId { get; set; }
        public string BranchCode { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public string TimeZoneId { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
