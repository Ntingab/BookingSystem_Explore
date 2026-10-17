using System;
using System.Threading;
using System.Threading.Tasks;
using BookingSystem_Explore.Infrastructure.Entities;

namespace BookingSystem_Explore.BusinessLibrary.Interfaces.Repositories
{
    public interface IAppointmentRepository
    {
        Task AddAsync(Appointment appointment, CancellationToken cancellationToken);
        Task<Appointment?> GetByIdAsync(Guid appointmentId, CancellationToken cancellationToken);
        Task<Appointment?> GetByReferenceAsync(string reference, CancellationToken cancellationToken);
        Task<bool> IsSlotOccupiedAsync(Guid appointmentSlotId, CancellationToken cancellationToken);
    }
}
