using System;
using BookingSystem_Explore.BusinessLibrary.DTOs.Requests;

namespace BookingSystem_Explore.BusinessLibrary.Validators
{
    public static class CancelAppointmentRequestValidator
    {
        public static void ValidateAndThrow(CancelAppointmentRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.ManagementToken)) throw new ArgumentException("Management token is required");
        }
    }
}
