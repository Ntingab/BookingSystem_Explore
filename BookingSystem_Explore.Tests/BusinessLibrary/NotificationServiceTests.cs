using BookingSystem_Explore.BusinessLibrary.Exceptions;
using BookingSystem_Explore.BusinessLibrary.Services;
using BookingSystem_Explore.Domain.Entities;
using BookingSystem_Explore.Domain.Enums;
using BookingSystem_Explore.Domain.Interfaces.Repositories;
using BookingSystem_Explore.Domain.Security;
using Moq;
using Xunit;

namespace BookingSystem_Explore.Tests.BusinessLibrary
{
    public class NotificationServiceTests
    {
        private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;
        private readonly Mock<INotificationRepository> _notificationRepositoryMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly NotificationService _sut;

        private readonly CancellationToken _ct = CancellationToken.None;

        public NotificationServiceTests()
        {
            _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
            _notificationRepositoryMock = new Mock<INotificationRepository>();
            _tokenServiceMock = new Mock<ITokenService>();

            _sut = new NotificationService(
                _appointmentRepositoryMock.Object,
                _notificationRepositoryMock.Object,
                _tokenServiceMock.Object);
        }

        #region GetAppointmentNotificationsAsync

        [Fact]
        public async Task GetAppointmentNotificationsAsync_WhenValidToken_ReturnsNotifications()
        {
            // Arrange
            var appointment = CreateAppointmentWithNotifications();

            SetupAppointmentLookup(appointment);
            SetupTokenVerification(true);

            // Act
            var result = await _sut.GetAppointmentNotificationsAsync(
                appointment.AppointmentId,
                "valid-token",
                _ct);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            _appointmentRepositoryMock.Verify(
                x => x.GetByIdAsync(appointment.AppointmentId, _ct),
                Times.Once);

            _tokenServiceMock.Verify(
                x => x.Verify("valid-token", "hashed-token"),
                Times.Once);
        }

        [Fact]
        public async Task GetAppointmentNotificationsAsync_WhenAppointmentNotFound_ReturnsEmptyList()
        {
            // Arrange
            var appointmentId = Guid.NewGuid();

            _appointmentRepositoryMock
                .Setup(x => x.GetByIdAsync(appointmentId, _ct))
                .ReturnsAsync((Appointment?)null);

            // Act
            var result = await _sut.GetAppointmentNotificationsAsync(
                appointmentId,
                "valid-token",
                _ct);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            _tokenServiceMock.Verify(
                x => x.Verify(
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task GetAppointmentNotificationsAsync_WhenInvalidToken_ThrowsUnauthorized()
        {
            // Arrange
            var appointment = CreateAppointmentWithNotifications();

            SetupAppointmentLookup(appointment);
            SetupTokenVerification(false);

            // Act
            var exception = await Assert.ThrowsAsync<
                UnauthorizedAppointmentAccessException>(
                () => _sut.GetAppointmentNotificationsAsync(
                    appointment.AppointmentId,
                    "invalid-token",
                    _ct));

            // Assert
            Assert.Equal("Invalid management token", exception.Message);
        }

        [Fact]
        public async Task GetAppointmentNotificationsAsync_WhenTokenHashIsEmpty_ThrowsUnauthorized()
        {
            // Arrange
            var appointment = CreateAppointmentWithNotifications();
            appointment.ManagementTokenHash = string.Empty;

            SetupAppointmentLookup(appointment);

            // Act & Assert
            await Assert.ThrowsAsync<
                UnauthorizedAppointmentAccessException>(
                () => _sut.GetAppointmentNotificationsAsync(
                    appointment.AppointmentId,
                    "valid-token",
                    _ct));

            _tokenServiceMock.Verify(
                x => x.Verify(
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task GetAppointmentNotificationsAsync_WhenTokenHashIsNull_ThrowsUnauthorized()
        {
            // Arrange
            var appointment = CreateAppointmentWithNotifications();
            appointment.ManagementTokenHash = null!;

            SetupAppointmentLookup(appointment);

            // Act & Assert
            await Assert.ThrowsAsync<
                UnauthorizedAppointmentAccessException>(
                () => _sut.GetAppointmentNotificationsAsync(
                    appointment.AppointmentId,
                    "valid-token",
                    _ct));

            _tokenServiceMock.Verify(
                x => x.Verify(
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task GetAppointmentNotificationsAsync_WhenNotificationsNull_ReturnsEmptyList()
        {
            // Arrange
            var appointment = CreateAppointment();
            appointment.Notifications = null!;

            SetupAppointmentLookup(appointment);
            SetupTokenVerification(true);

            // Act
            var result = await _sut.GetAppointmentNotificationsAsync(
                appointment.AppointmentId,
                "valid-token",
                _ct);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAppointmentNotificationsAsync_WhenNoNotifications_ReturnsEmptyList()
        {
            // Arrange
            var appointment = CreateAppointment();
            appointment.Notifications = new List<Notification>();

            SetupAppointmentLookup(appointment);
            SetupTokenVerification(true);

            // Act
            var result = await _sut.GetAppointmentNotificationsAsync(
                appointment.AppointmentId,
                "valid-token",
                _ct);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAppointmentNotificationsAsync_WhenTokenValid_MapsNotificationFieldsCorrectly()
        {
            // Arrange
            var appointment = CreateAppointmentWithNotifications();
            SetupAppointmentLookup(appointment);
            SetupTokenVerification(true);

            var expected = appointment.Notifications!.First();

            // Act
            var result = await _sut.GetAppointmentNotificationsAsync(
                appointment.AppointmentId,
                "valid-token",
                _ct);

            // Assert
            var notification = Assert.Single(
                result.Where(x =>
                    GetProperty<Guid>(x, "NotificationId") ==
                    expected.NotificationId));

            Assert.Equal(
                expected.NotificationId,
                GetProperty<Guid>(notification, "NotificationId"));

            Assert.Equal(
                expected.AppointmentId,
                GetProperty<Guid>(notification, "AppointmentId"));

            Assert.Equal(
                expected.NotificationType.ToString(),
                GetProperty<string>(notification, "Type"));

            Assert.Equal(
                expected.Status.ToString(),
                GetProperty<string>(notification, "Status"));

            Assert.Equal(
                expected.Channel.ToString(),
                GetProperty<string>(notification, "Channel"));

            Assert.Equal(
                expected.Recipient,
                GetProperty<string>(notification, "Recipient"));

            Assert.Equal(
                expected.Message,
                GetProperty<string>(notification, "Message"));

            Assert.Equal(
                expected.CreatedDate,
                GetProperty<DateTime>(notification, "CreatedDate"));

            Assert.Equal(
                expected.SentDate,
                GetProperty<DateTime?>(notification, "SentDate"));

            Assert.Equal(
                expected.ErrorMessage,
                GetProperty<string>(notification, "ErrorMessage"));
        }

        [Fact]
        public async Task GetAppointmentNotificationsAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            var appointmentId = Guid.NewGuid();

            _appointmentRepositoryMock
                .Setup(x => x.GetByIdAsync(appointmentId, _ct))
                .ThrowsAsync(new InvalidOperationException(
                    "Database connection failed"));

            // Act
            var exception = await Assert.ThrowsAsync<
                InvalidOperationException>(
                () => _sut.GetAppointmentNotificationsAsync(
                    appointmentId,
                    "valid-token",
                    _ct));

            // Assert
            Assert.Equal("Database connection failed", exception.Message);
        }

        [Fact]
        public async Task GetAppointmentNotificationsAsync_WhenTokenVerificationThrows_PropagatesException()
        {
            // Arrange
            var appointment = CreateAppointment();

            SetupAppointmentLookup(appointment);

            _tokenServiceMock
                .Setup(x => x.Verify("valid-token", "hashed-token"))
                .Throws(new InvalidOperationException(
                    "Token verification failed"));

            // Act
            var exception = await Assert.ThrowsAsync<
                InvalidOperationException>(
                () => _sut.GetAppointmentNotificationsAsync(
                    appointment.AppointmentId,
                    "valid-token",
                    _ct));

            // Assert
            Assert.Equal("Token verification failed", exception.Message);
        }

        [Fact]
        public async Task GetAppointmentNotificationsAsync_PassesCancellationTokenToRepository()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;

            var appointment = CreateAppointment();

            _appointmentRepositoryMock
                .Setup(x => x.GetByIdAsync(
                    appointment.AppointmentId,
                    cancellationToken))
                .ReturnsAsync(appointment);

            SetupTokenVerification(true);

            // Act
            await _sut.GetAppointmentNotificationsAsync(
                appointment.AppointmentId,
                "valid-token",
                cancellationToken);

            // Assert
            _appointmentRepositoryMock.Verify(
                x => x.GetByIdAsync(
                    appointment.AppointmentId,
                    cancellationToken),
                Times.Once);
        }

        [Fact]
        public async Task GetAppointmentNotificationsAsync_WhenInvalidToken_DoesNotReturnNotifications()
        {
            // Arrange
            var appointment = CreateAppointmentWithNotifications();

            SetupAppointmentLookup(appointment);
            SetupTokenVerification(false);

            // Act & Assert
            await Assert.ThrowsAsync<
                UnauthorizedAppointmentAccessException>(
                () => _sut.GetAppointmentNotificationsAsync(
                    appointment.AppointmentId,
                    "invalid-token",
                    _ct));

            _tokenServiceMock.Verify(
                x => x.Verify("invalid-token", "hashed-token"),
                Times.Once);
        }

        #endregion

        #region Helpers

        private void SetupAppointmentLookup(Appointment appointment)
        {
            _appointmentRepositoryMock
                .Setup(x => x.GetByIdAsync(
                    appointment.AppointmentId, _ct))
                .ReturnsAsync(appointment);
        }

        private void SetupTokenVerification(bool isValid)
        {
            _tokenServiceMock
                .Setup(x => x.Verify(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(isValid);
        }

        private static Appointment CreateAppointment()
        {
            return new Appointment
            {
                AppointmentId = Guid.NewGuid(),
                AppointmentReference = "APT-12345678",
                ManagementTokenHash = "hashed-token",
                Status = AppointmentStatus.Confirmed,
                CreatedDate = DateTime.UtcNow,
                Notifications = new List<Notification>()
            };
        }

        private static Appointment CreateAppointmentWithNotifications()
        {
            var appointment = CreateAppointment();

            appointment.Notifications = new List<Notification>
            {
                new Notification
                {
                    NotificationId = Guid.NewGuid(),
                    AppointmentId = appointment.AppointmentId,
                    NotificationType = NotificationType.BookingConfirmation,
                    Status = NotificationStatus.Pending,
                    Channel = NotificationChannel.Email,
                    Recipient = "customer@test.com",
                    Message = "Your appointment has been confirmed",
                    CreatedDate = DateTime.UtcNow,
                    SentDate = null,
                    ErrorMessage = null
                },
                new Notification
                {
                    NotificationId = Guid.NewGuid(),
                    AppointmentId = appointment.AppointmentId,
                    NotificationType = NotificationType.BookingCancellation,
                    Status = NotificationStatus.Pending,
                    Channel = NotificationChannel.Email,
                    Recipient = "customer@test.com",
                    Message = "Appointment cancellation notification",
                    CreatedDate = DateTime.UtcNow,
                    SentDate = null,
                    ErrorMessage = null
                }
            };

            return appointment;
        }

        // NotificationService returns anonymous objects.
        // Reflection allows us to verify their properties.
        private static T GetProperty<T>(object obj, string propertyName)
        {
            var property = obj.GetType().GetProperty(propertyName);

            Assert.NotNull(property);

            var value = property.GetValue(obj);

            if (value == null)
                return default!;

            return (T)value;
        }

        #endregion
    }
}
