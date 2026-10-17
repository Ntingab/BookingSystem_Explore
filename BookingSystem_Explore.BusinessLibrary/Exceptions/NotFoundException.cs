using System;

namespace BookingSystem_Explore.BusinessLibrary.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }
}
