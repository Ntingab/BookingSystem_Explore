namespace BookingSystem_Explore.BusinessLibrary.Interfaces.Services
{
    public interface INotificationService
    {
        Task<IReadOnlyList<object>> GetAllNotificationsAsync(CancellationToken cancellationToken);
        Task<IReadOnlyList<object>> GetAppointmentNotificationsAsync(Guid appointmentId, string managementToken, CancellationToken cancellationToken);
    }
}
