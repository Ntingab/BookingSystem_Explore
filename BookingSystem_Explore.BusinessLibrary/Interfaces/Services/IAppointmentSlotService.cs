using BookingSystem_Explore.BusinessLibrary.DTOs.Responses;

namespace BookingSystem_Explore.BusinessLibrary.Interfaces.Services
{
    public interface IAppointmentSlotService
    {
        Task<IReadOnlyList<AppointmentSlotResponse>> GetAllAvailableSlotsAsync(CancellationToken cancellationToken);
        Task<IReadOnlyList<AppointmentSlotResponse>> GetAvailableSlotsAsync(Guid branchServiceId, DateOnly date, CancellationToken cancellationToken);
        Task<AppointmentSlotResponse?> GetSlotByIdAsync(Guid slotId, CancellationToken cancellationToken);
        Task<AppointmentSlotResponse> CreateSlotAsync(BookingSystem_Explore.BusinessLibrary.DTOs.Requests.CreateAppointmentSlotRequest request, CancellationToken cancellationToken);
        Task<AppointmentSlotResponse?> UpdateSlotAsync(Guid slotId, BookingSystem_Explore.BusinessLibrary.DTOs.Requests.UpdateAppointmentSlotRequest request, CancellationToken cancellationToken);
    }
}
