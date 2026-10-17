using BookingSystem_Explore.Domain.Entities;
using BookingSystem_Explore.Domain.Enums;
using BookingSystem_Explore.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace BookingSystem_Explore.BusinessLibrary.Notifications
{
    public class SimulatedNotificationProcessor
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly ILogger<SimulatedNotificationProcessor> _logger;

        public SimulatedNotificationProcessor(INotificationRepository notificationRepository, ILogger<SimulatedNotificationProcessor> logger)
        {
            _notificationRepository = notificationRepository;
            _logger = logger;
        }

        public async Task ProcessNotificationAsync(Notification notification, CancellationToken cancellationToken)
        {
            try
            {
                notification.SentDate = DateTime.UtcNow;
                notification.Status = NotificationStatus.SimulatedSent.ToString();
                await _notificationRepository.UpdateAsync(notification, cancellationToken);
                _logger.LogInformation("Simulated notification {NotificationId} sent", notification.NotificationId);
            }
            catch (Exception ex)
            {
                notification.ErrorMessage = ex.Message;
                notification.Status = NotificationStatus.Failed.ToString();
                try { await _notificationRepository.UpdateAsync(notification, cancellationToken); } catch { }
                _logger.LogError(ex, "Simulated notification failed {NotificationId}", notification.NotificationId);
            }
        }
    }
}
