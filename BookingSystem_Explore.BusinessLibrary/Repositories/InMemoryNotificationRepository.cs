using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BookingSystem_Explore.Domain.Interfaces.Repositories;
using BookingSystem_Explore.Domain.Entities;

namespace BookingSystem_Explore.BusinessLibrary.Repositories
{
    public class InMemoryNotificationRepository : INotificationRepository
    {
        private readonly InMemoryDataStore _store;

        public InMemoryNotificationRepository(InMemoryDataStore store)
        {
            _store = store;
        }

        public Task AddAsync(Notification notification, CancellationToken cancellationToken)
        {
            _store.Notifications.Add(notification);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Notification notification, CancellationToken cancellationToken)
        {
            var existing = _store.Notifications.FirstOrDefault(n => n.NotificationId == notification.NotificationId);
            if (existing != null)
            {
                existing.SentDate = notification.SentDate;
                existing.Status = notification.Status;
                existing.ErrorMessage = notification.ErrorMessage;
                existing.Message = notification.Message;
            }
            return Task.CompletedTask;
        }
    }
}
