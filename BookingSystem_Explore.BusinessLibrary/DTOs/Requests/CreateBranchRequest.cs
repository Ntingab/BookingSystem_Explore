using System.ComponentModel.DataAnnotations;

namespace BookingSystem_Explore.BusinessLibrary.DTOs.Requests;

public class CreateBranchRequest
{
    [Required, MaxLength(20)]
    public string BranchCode { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string BranchName { get; set; } = string.Empty;

    [Required, MaxLength(250)]
    public string Address { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Province { get; set; } = string.Empty;

    [MaxLength(30)]
    public string ContactNumber { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string TimeZoneId { get; set; } = "Africa/Johannesburg";

    public bool IsActive { get; set; } = true;
}
