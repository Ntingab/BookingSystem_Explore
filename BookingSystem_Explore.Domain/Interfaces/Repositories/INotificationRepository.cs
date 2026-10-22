using BookingSystem_Explore.Domain.Entities;

namespace BookingSystem_Explore.Domain.Interfaces.Repositories
{
    public interface INotificationRepository
    {
        Task<IReadOnlyList<Notification>> GetAllAsync(CancellationToken cancellationToken);
        Task AddAsync(Notification notification, CancellationToken cancellationToken);
        Task UpdateAsync(Notification notification, CancellationToken cancellationToken);
    }
}
