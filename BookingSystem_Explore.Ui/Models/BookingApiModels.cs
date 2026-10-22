namespace BookingSystem_Explore.Ui.Models;

public class BranchResponse
{
    public Guid BranchId { get; set; }
    public string BranchCode { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
}

public class ServiceResponse
{
    public Guid ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
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
    public DateTime StartDateTimeUtc { get; set; }
    public DateTime EndDateTimeUtc { get; set; }
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
