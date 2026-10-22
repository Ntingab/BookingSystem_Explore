using BookingSystem_Explore.Domain.Entities;

namespace BookingSystem_Explore.Domain.Interfaces.Repositories
{
    public interface IServiceRepository
    {
        Task<IReadOnlyList<Service>> GetActiveServicesAsync(CancellationToken cancellationToken);
        Task<Service?> GetByIdAsync(Guid serviceId, CancellationToken cancellationToken);
        Task AddAsync(Service service, CancellationToken cancellationToken);
        Task UpdateAsync(Service service, CancellationToken cancellationToken);
    }
}
