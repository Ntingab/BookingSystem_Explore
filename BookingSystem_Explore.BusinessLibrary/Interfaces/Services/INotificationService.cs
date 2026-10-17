namespace BookingSystem_Explore.BusinessLibrary.Interfaces.Services
{
    public interface INotificationService
    {
        Task<IReadOnlyList<object>> GetAppointmentNotificationsAsync(Guid appointmentId, string managementToken, CancellationToken cancellationToken);
    }
}
