using System;

namespace BookingSystem_Explore.BusinessLibrary.DTOs.Requests
{
    public class CreateAppointmentRequest
    {
        public Guid BranchServiceId { get; set; }
        public Guid AppointmentSlotId { get; set; }
        public CustomerDto Customer { get; set; } = new CustomerDto();
        public string? Notes { get; set; }

        public class CustomerDto
        {
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
            public string EmailAddress { get; set; } = string.Empty;
            public string PhoneNumber { get; set; } = string.Empty;
        }
    }
}
