using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BookingSystem_Explore.Domain.Interfaces.Repositories;
using BookingSystem_Explore.Domain.Entities;

namespace BookingSystem_Explore.BusinessLibrary.Repositories
{
    public class InMemoryServiceRepository : IServiceRepository
    {
        private readonly InMemoryDataStore _store;

        public InMemoryServiceRepository(InMemoryDataStore store)
        {
            _store = store;
        }

        public Task<IReadOnlyList<Service>> GetActiveServicesAsync(CancellationToken cancellationToken)
        {
            var list = _store.Services.Where(s => s.IsActive).ToList();
            return Task.FromResult((IReadOnlyList<Service>)list);
        }

        public Task<Service?> GetByIdAsync(Guid serviceId, CancellationToken cancellationToken)
        {
            var s = _store.Services.FirstOrDefault(x => x.ServiceId == serviceId);
            return Task.FromResult(s);
        }
    }
}
