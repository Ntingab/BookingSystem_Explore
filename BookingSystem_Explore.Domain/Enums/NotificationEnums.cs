namespace BookingSystem_Explore.Domain.Enums
{
    public enum NotificationType
    {
        BookingConfirmation,
        BookingCancellation
    }

    public enum NotificationChannel
    {
        Email,
        SMS
    }

    public enum NotificationStatus
    {
        Pending,
        Sent,
        Failed,
        SimulatedSent
    }
}
