namespace BookingSystem_Explore.Ui.Models;

public class BranchResponse
{
    public Guid BranchId { get; set; }
    public string BranchCode { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    public string TimeZoneId { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class BranchRequest
{
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.MaxLength(20)]
    public string BranchCode { get; set; } = string.Empty;
    [System.ComponentModel.DataAnnotations.Required]
    public string BranchName { get; set; } = string.Empty;
    [System.ComponentModel.DataAnnotations.Required]
    public string Address { get; set; } = string.Empty;
    [System.ComponentModel.DataAnnotations.Required]
    public string City { get; set; } = string.Empty;
    [System.ComponentModel.DataAnnotations.Required]
    public string Province { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    [System.ComponentModel.DataAnnotations.Required]
    public string TimeZoneId { get; set; } = "Africa/Johannesburg";
    public bool IsActive { get; set; } = true;
}

public class ServiceResponse
{
    public Guid ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
}

public class CreateBranchServiceRequest
{
    public Guid ServiceId { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateBranchServiceRequest
{
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
}

public class BranchServiceResponse
{
    public Guid BranchServiceId { get; set; }
    public Guid BranchId { get; set; }
    public Guid ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
}

public class AppointmentSlotResponse
{
    public Guid AppointmentSlotId { get; set; }
    public Guid BranchServiceId { get; set; }
    public DateTime StartDateTimeUtc { get; set; }
    public DateTime EndDateTimeUtc { get; set; }
    public bool IsActive { get; set; }
}

public class CreateAppointmentSlotRequest
{
    public Guid BranchServiceId { get; set; }
    public DateTime StartDateTimeUtc { get; set; }
    public DateTime EndDateTimeUtc { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateAppointmentSlotRequest
{
    public DateTime StartDateTimeUtc { get; set; }
    public DateTime EndDateTimeUtc { get; set; }
    public bool IsActive { get; set; }
}

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
    public string ManagementToken { get; set; } = string.Empty;
}

public class CreateAppointmentRequest
{
    public Guid BranchServiceId { get; set; }
    public Guid AppointmentSlotId { get; set; }
    public CustomerDto Customer { get; set; } = new();
    public string? Notes { get; set; }

    public class CustomerDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
