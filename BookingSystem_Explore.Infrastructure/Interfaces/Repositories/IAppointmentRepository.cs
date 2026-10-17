using BookingSystem_Explore.Domain.Entities;

namespace BookingSystem_Explore.Domain.Interfaces.Repositories
{
    public interface IAppointmentRepository
    {
        Task AddAsync(Appointment appointment, CancellationToken cancellationToken);
        Task<Appointment?> GetByIdAsync(Guid appointmentId, CancellationToken cancellationToken);
        Task<Appointment?> GetByReferenceAsync(string reference, CancellationToken cancellationToken);
        Task<bool> IsSlotOccupiedAsync(Guid appointmentSlotId, CancellationToken cancellationToken);
    }
}
