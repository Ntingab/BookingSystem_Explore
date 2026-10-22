using BookingSystem_Explore.Domain.Entities;
using BookingSystem_Explore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

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

        public async Task<IReadOnlyList<Notification>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _db.Notifications
                .AsNoTracking()
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync(cancellationToken);
        }

        public async Task UpdateAsync(Notification notification, CancellationToken cancellationToken)
        {
            _db.Notifications.Update(notification);
            await Task.CompletedTask;
        }
    }
}
