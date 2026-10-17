using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BookingSystem_Explore.Infrastructure.Entities;

namespace BookingSystem_Explore.BusinessLibrary.Interfaces.Repositories
{
    public interface IServiceRepository
    {
        Task<IReadOnlyList<Service>> GetActiveServicesAsync(CancellationToken cancellationToken);
        Task<Service?> GetByIdAsync(Guid serviceId, CancellationToken cancellationToken);
    }
}
