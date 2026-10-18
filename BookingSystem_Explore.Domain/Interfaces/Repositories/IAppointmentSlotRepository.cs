using BookingSystem_Explore.Domain.Entities;

namespace BookingSystem_Explore.Domain.Interfaces.Repositories
{
    public interface IAppointmentSlotRepository
    {
        Task<AppointmentSlot?> GetByIdAsync(Guid slotId, CancellationToken cancellationToken);
        Task<IReadOnlyList<AppointmentSlot>> GetAvailableSlotsAsync(Guid branchServiceId, DateOnly date, CancellationToken cancellationToken);
        Task AddAsync(AppointmentSlot slot, CancellationToken cancellationToken);
        Task UpdateAsync(AppointmentSlot slot, CancellationToken cancellationToken);
    }
}
