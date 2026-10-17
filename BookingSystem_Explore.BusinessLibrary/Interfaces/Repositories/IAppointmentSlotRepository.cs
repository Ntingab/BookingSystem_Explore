using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BookingSystem_Explore.Infrastructure.Entities;

namespace BookingSystem_Explore.BusinessLibrary.Interfaces.Repositories
{
    public interface IAppointmentSlotRepository
    {
        Task<AppointmentSlot?> GetByIdAsync(Guid slotId, CancellationToken cancellationToken);
        Task<IReadOnlyList<AppointmentSlot>> GetAvailableSlotsAsync(Guid branchServiceId, DateOnly date, CancellationToken cancellationToken);
    }
}
