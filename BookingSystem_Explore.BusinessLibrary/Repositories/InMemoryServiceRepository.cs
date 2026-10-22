using BookingSystem_Explore.Domain.Entities;
using BookingSystem_Explore.Domain.Interfaces.Repositories;

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

        public Task AddAsync(Service service, CancellationToken cancellationToken)
        {
            _store.Services.Add(service);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Service service, CancellationToken cancellationToken)
        {
            var existing = _store.Services.FirstOrDefault(x => x.ServiceId == service.ServiceId);
            if (existing != null)
            {
                existing.ServiceName = service.ServiceName;
                existing.DurationMinutes = service.DurationMinutes;
                existing.Description = service.Description;
                existing.IsActive = service.IsActive;
                existing.UpdatedDate = service.UpdatedDate;
            }

            return Task.CompletedTask;
        }
    }
}
