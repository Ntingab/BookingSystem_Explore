using System;

namespace BookingSystem_Explore.BusinessLibrary.Exceptions
{
    public class UnauthorizedAppointmentAccessException : Exception
    {
        public UnauthorizedAppointmentAccessException()
        {
        }

        public UnauthorizedAppointmentAccessException(string message) : base(message)
        {
        }
    }
}
