namespace BookingSystem_Explore.Infrastructure.Enums
{
    public enum NotificationType
    {
        BookingConfirmation = 1,
        BookingCancellation = 2
    }

    public enum NotificationChannel
    {
        Email = 1,
        SMS = 2
    }

    public enum NotificationStatus
    {
        Pending = 1,
        SimulatedSent = 2,
        Failed = 3
    }
}
