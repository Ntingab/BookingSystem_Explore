using BookingSystem_Explore.BusinessLibrary.DTOs.Responses;

namespace BookingSystem_Explore.BusinessLibrary.Interfaces.Services
{
    public interface IAppointmentSlotService
    {
        Task<IReadOnlyList<AppointmentSlotResponse>> GetAvailableSlotsAsync(Guid branchServiceId, DateOnly date, CancellationToken cancellationToken);
        Task<AppointmentSlotResponse?> GetSlotByIdAsync(Guid slotId, CancellationToken cancellationToken);
    }
}
