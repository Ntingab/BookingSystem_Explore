using BookingSystem_Explore.BusinessLibrary.Interfaces.Services;
using BookingSystem_Explore.Domain.Interfaces.Repositories;
using BookingSystem_Explore.Domain.Security;

namespace BookingSystem_Explore.BusinessLibrary.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly ITokenService _tokenService;

        public NotificationService(IAppointmentRepository appointmentRepository, INotificationRepository notificationRepository, ITokenService tokenService)
        {
            _appointmentRepository = appointmentRepository;
            _notificationRepository = notificationRepository;
            _tokenService = tokenService;
        }

        public async Task<IReadOnlyList<object>> GetAppointmentNotificationsAsync(Guid appointmentId, string managementToken, CancellationToken cancellationToken)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId, cancellationToken);
            if (appointment == null) return Array.Empty<object>();

            var hash = appointment.ManagementTokenHash;
            if (string.IsNullOrEmpty(hash) || !_tokenService.Verify(managementToken, hash))
            {
                throw new BookingSystem_Explore.BusinessLibrary.Exceptions.UnauthorizedAppointmentAccessException("Invalid management token");
            }

            var list = appointment.Notifications?.Select(ToResponse).ToList() ?? new List<object>();

            return list;
        }

        private static object ToResponse(BookingSystem_Explore.Domain.Entities.Notification notification)
        {
            return new
            {
                notification.NotificationId,
                notification.AppointmentId,
                Type = notification.NotificationType.ToString(),
                Status = notification.Status.ToString(),
                Channel = notification.Channel.ToString(),
                notification.Recipient,
                notification.Message,
                notification.CreatedDate,
                notification.SentDate,
                notification.ErrorMessage
            };
        }
    }
}
