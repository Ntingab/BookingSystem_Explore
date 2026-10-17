using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BookingSystem_Explore.Domain.Interfaces.Repositories;
using BookingSystem_Explore.Domain.Entities;
using BookingSystem_Explore.Infrastructure.Persistence;

namespace BookingSystem_Explore.Infrastructure.Repositories
{
    public class NotificationRepository : BookingSystem_Explore.Domain.Interfaces.Repositories.INotificationRepository
    {
        private readonly AppointmentBookingDbContext _db;

        public NotificationRepository(AppointmentBookingDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(Notification notification, CancellationToken cancellationToken)
        {
            await _db.Notifications.AddAsync(notification, cancellationToken);
        }

        public async Task UpdateAsync(Notification notification, CancellationToken cancellationToken)
        {
            _db.Notifications.Update(notification);
            await Task.CompletedTask;
        }
    }
}
