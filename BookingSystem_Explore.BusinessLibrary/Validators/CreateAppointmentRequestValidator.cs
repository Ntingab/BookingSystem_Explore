using System;
using BookingSystem_Explore.BusinessLibrary.DTOs.Requests;

namespace BookingSystem_Explore.BusinessLibrary.Validators
{
    public static class CreateAppointmentRequestValidator
    {
        public static void ValidateAndThrow(CreateAppointmentRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.BranchServiceId == Guid.Empty) throw new ArgumentException("BranchServiceId is required");
            if (request.AppointmentSlotId == Guid.Empty) throw new ArgumentException("AppointmentSlotId is required");
            if (request.Customer == null) throw new ArgumentException("Customer is required");
            if (string.IsNullOrWhiteSpace(request.Customer.FirstName) || request.Customer.FirstName.Length > 100) throw new ArgumentException("Invalid customer first name");
            if (string.IsNullOrWhiteSpace(request.Customer.LastName) || request.Customer.LastName.Length > 100) throw new ArgumentException("Invalid customer last name");
            if (string.IsNullOrWhiteSpace(request.Customer.EmailAddress) || request.Customer.EmailAddress.Length > 254) throw new ArgumentException("Invalid customer email");
            if (string.IsNullOrWhiteSpace(request.Customer.PhoneNumber) || request.Customer.PhoneNumber.Length > 20) throw new ArgumentException("Invalid customer phone");
            if (request.Notes != null && request.Notes.Length > 500) throw new ArgumentException("Notes too long");
        }
    }
}
