using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BookingSystem_Explore.Domain.Entities;

namespace BookingSystem_Explore.Domain.Interfaces.Repositories
{
    public interface IAppointmentSlotRepository
    {
        Task<AppointmentSlot?> GetByIdAsync(Guid slotId, CancellationToken cancellationToken);
        Task<IReadOnlyList<AppointmentSlot>> GetAvailableSlotsAsync(Guid branchServiceId, DateOnly date, CancellationToken cancellationToken);
    }
}
