using BookingSystem_Explore.BusinessLibrary.DTOs.Requests;
using BookingSystem_Explore.BusinessLibrary.Exceptions;
using BookingSystem_Explore.BusinessLibrary.Services;
using BookingSystem_Explore.Domain.Entities;
using BookingSystem_Explore.Domain.Enums;
using BookingSystem_Explore.Domain.Interfaces;
using BookingSystem_Explore.Domain.Interfaces.Repositories;
using BookingSystem_Explore.Domain.Security;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Xunit;
using BranchEntity = BookingSystem_Explore.Domain.Entities.Branch;
using BranchServiceEntity = BookingSystem_Explore.Domain.Entities.BranchService;
using ServiceEntity = BookingSystem_Explore.Domain.Entities.Service;

namespace BookingSystem_Explore.Tests.BusinessLibrary
{
    public class AppointmentServiceTests
    {
        private readonly Mock<IAppointmentRepository> _appointmentRepository;
        private readonly Mock<IAppointmentSlotRepository> _slotRepository;
        private readonly Mock<IBranchRepository> _branchRepository;
        private readonly Mock<IServiceRepository> _serviceRepository;
        private readonly Mock<ICustomerRepository> _customerRepository;
        private readonly Mock<INotificationRepository> _notificationRepository;
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<ITokenService> _tokenService;
        private readonly Mock<ILogger<AppointmentService>> _logger;

        private readonly AppointmentService _sut;

        private readonly CancellationToken _ct = CancellationToken.None;

        public AppointmentServiceTests()
        {
            _appointmentRepository = new Mock<IAppointmentRepository>();
            _slotRepository = new Mock<IAppointmentSlotRepository>();
            _branchRepository = new Mock<IBranchRepository>();
            _serviceRepository = new Mock<IServiceRepository>();
            _customerRepository = new Mock<ICustomerRepository>();
            _notificationRepository = new Mock<INotificationRepository>();
            _unitOfWork = new Mock<IUnitOfWork>();
            _tokenService = new Mock<ITokenService>();
            _logger = new Mock<ILogger<AppointmentService>>();

            _sut = new AppointmentService(
                _appointmentRepository.Object,
                _slotRepository.Object,
                _branchRepository.Object,
                _serviceRepository.Object,
                _customerRepository.Object,
                _notificationRepository.Object,
                _unitOfWork.Object,
                _tokenService.Object,
                _logger.Object);
        }

        #region BookAppointmentAsync

        [Fact]
        public async Task BookAppointmentAsync_WhenValid_ReturnsConfirmedAppointment()
        {
            // Arrange
            var slot = SetupValidBooking();
            var request = CreateRequest(slot);

            _customerRepository
                .Setup(x => x.GetByEmailAddressAsync(
                    It.IsAny<string>(), _ct))
                .ReturnsAsync(CreateCustomer());

            _appointmentRepository
                .Setup(x => x.IsSlotOccupiedAsync(
                    slot.AppointmentSlotId, _ct))
                .ReturnsAsync(false);

            SetupToken();

            // Act
            var result = await _sut.BookAppointmentAsync(request, _ct);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.AppointmentId);
            Assert.StartsWith("APT-", result.AppointmentReference);
            Assert.Equal("Confirmed", result.Status);
            Assert.Equal("Johannesburg Branch", result.BranchName);
            Assert.Equal("Haircut", result.ServiceName);
            Assert.Equal("Pending", result.NotificationStatus);
            Assert.Equal("test-management-token", result.ManagementToken);

            _appointmentRepository.Verify(
                x => x.AddAsync(
                    It.Is<Appointment>(a =>
                        a.AppointmentSlotId == slot.AppointmentSlotId &&
                        a.Status == AppointmentStatus.Confirmed &&
                        a.ManagementTokenHash == "hashed-token"),
                    _ct),
                Times.Once);

            _notificationRepository.Verify(
                x => x.AddAsync(
                    It.Is<Notification>(n =>
                        n.NotificationType == NotificationType.BookingConfirmation &&
                        n.Status == NotificationStatus.Pending),
                    _ct),
                Times.Once);

            VerifySavedOnce();
        }

        [Fact]
        public async Task BookAppointmentAsync_WhenSlotNotFound_ThrowsException()
        {
            // Arrange
            var request = CreateRequest();

            _slotRepository
                .Setup(x => x.GetByIdAsync(request.AppointmentSlotId, _ct))
                .ReturnsAsync((AppointmentSlot?)null);

            // Act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.BookAppointmentAsync(request, _ct));

            // Assert
            Assert.Equal("Slot not found", ex.Message);
            VerifyNeverSaved();
        }

        [Fact]
        public async Task BookAppointmentAsync_WhenBranchServiceMissing_ThrowsException()
        {
            // Arrange
            var slot = CreateSlot();
            slot.BranchService = null;

            _slotRepository
                .Setup(x => x.GetByIdAsync(slot.AppointmentSlotId, _ct))
                .ReturnsAsync(slot);

            // Act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.BookAppointmentAsync(
                    CreateRequest(slot), _ct));

            // Assert
            Assert.Equal("Slot missing branch-service info", ex.Message);
            VerifyNeverSaved();
        }

        [Fact]
        public async Task BookAppointmentAsync_WhenBranchInactive_ThrowsException()
        {
            // Arrange
            var slot = SetupValidBooking(branchActive: false);

            // Act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.BookAppointmentAsync(
                    CreateRequest(slot), _ct));

            // Assert
            Assert.Equal("Branch not active or not found", ex.Message);
            VerifyNeverSaved();
        }

        [Fact]
        public async Task BookAppointmentAsync_WhenServiceInactive_ThrowsException()
        {
            // Arrange
            var slot = SetupValidBooking(serviceActive: false);

            // Act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.BookAppointmentAsync(
                    CreateRequest(slot), _ct));

            // Assert
            Assert.Equal("Service not active or not found", ex.Message);
            VerifyNeverSaved();
        }

        [Fact]
        public async Task BookAppointmentAsync_WhenSlotInactive_ThrowsException()
        {
            // Arrange
            var slot = SetupValidBooking();
            slot.IsActive = false;

            // Act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.BookAppointmentAsync(
                    CreateRequest(slot), _ct));

            // Assert
            Assert.Equal("Slot not available", ex.Message);
            VerifyNeverSaved();
        }

        [Fact]
        public async Task BookAppointmentAsync_WhenSlotInPast_ThrowsException()
        {
            // Arrange
            var slot = SetupValidBooking();
            slot.StartDateTimeUtc = DateTime.UtcNow.AddHours(-2);

            // Act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.BookAppointmentAsync(
                    CreateRequest(slot), _ct));

            // Assert
            Assert.Equal("Slot not available", ex.Message);
            VerifyNeverSaved();
        }

        [Fact]
        public async Task BookAppointmentAsync_WhenSlotOccupied_ThrowsException()
        {
            // Arrange
            var slot = SetupValidBooking();

            _appointmentRepository
                .Setup(x => x.IsSlotOccupiedAsync(
                    slot.AppointmentSlotId, _ct))
                .ReturnsAsync(true);

            // Act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.BookAppointmentAsync(
                    CreateRequest(slot), _ct));

            // Assert
            Assert.Equal("Slot already occupied", ex.Message);
            VerifyNeverSaved();
        }

        [Fact]
        public async Task BookAppointmentAsync_WhenCustomerExists_DoesNotCreateCustomer()
        {
            // Arrange
            var slot = SetupValidBooking();

            _customerRepository
                .Setup(x => x.GetByEmailAddressAsync(
                    "customer@test.com", _ct))
                .ReturnsAsync(CreateCustomer());

            SetupToken();

            // Act
            await _sut.BookAppointmentAsync(CreateRequest(slot), _ct);

            // Assert
            _customerRepository.Verify(
                x => x.AddAsync(
                    It.IsAny<Customer>(), _ct),
                Times.Never);

            VerifySavedOnce();
        }

        [Fact]
        public async Task BookAppointmentAsync_WhenCustomerDoesNotExist_CreatesCustomer()
        {
            // Arrange
            var slot = SetupValidBooking();

            _customerRepository
                .Setup(x => x.GetByEmailAddressAsync(
                    "customer@test.com", _ct))
                .ReturnsAsync((Customer?)null);

            SetupToken();

            // Act
            var result = await _sut.BookAppointmentAsync(
                CreateRequest(slot), _ct);

            // Assert
            Assert.NotNull(result);

            _customerRepository.Verify(
                x => x.AddAsync(
                    It.Is<Customer>(c =>
                        c.FirstName == "John" &&
                        c.LastName == "Smith" &&
                        c.EmailAddress == "customer@test.com"),
                    _ct),
                Times.Once);

            VerifySavedOnce();
        }

        [Fact]
        public async Task BookAppointmentAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            var request = CreateRequest();

            _slotRepository
                .Setup(x => x.GetByIdAsync(
                    request.AppointmentSlotId, _ct))
                .ThrowsAsync(new InvalidOperationException(
                    "Database connection failed"));

            // Act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.BookAppointmentAsync(request, _ct));

            // Assert
            Assert.Equal("Database connection failed", ex.Message);
            VerifyNeverSaved();
        }

        #endregion

        #region GetAppointmentAsync

        [Fact]
        public async Task GetAppointmentAsync_WhenValidToken_ReturnsAppointment()
        {
            // Arrange
            var appointment = CreateAppointment();

            _appointmentRepository
                .Setup(x => x.GetByIdAsync(appointment.AppointmentId, _ct))
                .ReturnsAsync(appointment);

            SetupTokenVerification(true);

            // Act
            var result = await _sut.GetAppointmentAsync(
                appointment.AppointmentId,
                "valid-token",
                _ct);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(appointment.AppointmentId, result.AppointmentId);
            Assert.Equal(appointment.AppointmentReference,
                result.AppointmentReference);
            Assert.Equal("Confirmed", result.Status);
            Assert.Equal("Johannesburg Branch", result.BranchName);
            Assert.Equal("Haircut", result.ServiceName);
        }

        [Fact]
        public async Task GetAppointmentAsync_WhenNotFound_ReturnsNull()
        {
            // Arrange
            var appointmentId = Guid.NewGuid();

            _appointmentRepository
                .Setup(x => x.GetByIdAsync(appointmentId, _ct))
                .ReturnsAsync((Appointment?)null);

            // Act
            var result = await _sut.GetAppointmentAsync(
                appointmentId, "valid-token", _ct);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAppointmentAsync_WhenInvalidToken_ThrowsUnauthorized()
        {
            // Arrange
            var appointment = CreateAppointment();

            _appointmentRepository
                .Setup(x => x.GetByIdAsync(appointment.AppointmentId, _ct))
                .ReturnsAsync(appointment);

            SetupTokenVerification(false);

            // Act & Assert
            await Assert.ThrowsAsync<
                UnauthorizedAppointmentAccessException>(
                () => _sut.GetAppointmentAsync(
                    appointment.AppointmentId,
                    "invalid-token",
                    _ct));
        }

        #endregion

        #region GetAppointmentByReferenceAsync

        [Fact]
        public async Task GetAppointmentByReferenceAsync_WhenValid_ReturnsAppointment()
        {
            // Arrange
            var appointment = CreateAppointment();

            _appointmentRepository
                .Setup(x => x.GetByReferenceAsync(
                    appointment.AppointmentReference, _ct))
                .ReturnsAsync(appointment);

            SetupTokenVerification(true);

            // Act
            var result = await _sut.GetAppointmentByReferenceAsync(
                appointment.AppointmentReference,
                "valid-token",
                _ct);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(appointment.AppointmentId, result.AppointmentId);
            Assert.Equal(appointment.AppointmentReference,
                result.AppointmentReference);
        }

        [Fact]
        public async Task GetAppointmentByReferenceAsync_WhenNotFound_ReturnsNull()
        {
            // Arrange
            _appointmentRepository
                .Setup(x => x.GetByReferenceAsync(
                    "APT-NOTFOUND", _ct))
                .ReturnsAsync((Appointment?)null);

            // Act
            var result = await _sut.GetAppointmentByReferenceAsync(
                "APT-NOTFOUND", "valid-token", _ct);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAppointmentByReferenceAsync_WhenInvalidToken_ThrowsUnauthorized()
        {
            // Arrange
            var appointment = CreateAppointment();

            _appointmentRepository
                .Setup(x => x.GetByReferenceAsync(
                    appointment.AppointmentReference, _ct))
                .ReturnsAsync(appointment);

            SetupTokenVerification(false);

            // Act & Assert
            await Assert.ThrowsAsync<
                UnauthorizedAppointmentAccessException>(
                () => _sut.GetAppointmentByReferenceAsync(
                    appointment.AppointmentReference,
                    "invalid-token",
                    _ct));
        }

        #endregion

        #region CancelAppointmentAsync

        [Fact]
        public async Task CancelAppointmentAsync_WhenValid_CancelsAppointment()
        {
            // Arrange
            var appointment = CreateAppointment();
            SetupAppointmentLookup(appointment);
            SetupTokenVerification(true);

            // Act
            var result = await _sut.CancelAppointmentAsync(
                appointment.AppointmentId,
                "valid-token",
                _ct);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Cancelled", result.Status);
            Assert.Equal(AppointmentStatus.Cancelled, appointment.Status);
            Assert.NotNull(appointment.CancelledDate);
            Assert.NotNull(appointment.UpdatedDate);

            _notificationRepository.Verify(
                x => x.AddAsync(
                    It.Is<Notification>(n =>
                        n.AppointmentId == appointment.AppointmentId &&
                        n.NotificationType == NotificationType.BookingCancellation),
                    _ct),
                Times.Once);

            VerifySavedOnce();
        }

        [Fact]
        public async Task CancelAppointmentAsync_WhenNotFound_ThrowsNotFound()
        {
            // Arrange
            var appointmentId = Guid.NewGuid();

            _appointmentRepository
                .Setup(x => x.GetByIdAsync(appointmentId, _ct))
                .ReturnsAsync((Appointment?)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(
                () => _sut.CancelAppointmentAsync(
                    appointmentId, "valid-token", _ct));

            VerifyNeverSaved();
        }

        [Fact]
        public async Task CancelAppointmentAsync_WhenInvalidToken_ThrowsUnauthorized()
        {
            // Arrange
            var appointment = CreateAppointment();
            SetupAppointmentLookup(appointment);
            SetupTokenVerification(false);

            // Act & Assert
            await Assert.ThrowsAsync<
                UnauthorizedAppointmentAccessException>(
                () => _sut.CancelAppointmentAsync(
                    appointment.AppointmentId,
                    "invalid-token",
                    _ct));

            Assert.Equal(AppointmentStatus.Confirmed, appointment.Status);
            VerifyNeverSaved();
        }

        [Fact]
        public async Task CancelAppointmentAsync_WhenAlreadyCancelled_ThrowsException()
        {
            // Arrange
            var appointment = CreateAppointment();
            appointment.Status = AppointmentStatus.Cancelled;

            SetupAppointmentLookup(appointment);
            SetupTokenVerification(true);

            // Act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.CancelAppointmentAsync(
                    appointment.AppointmentId,
                    "valid-token",
                    _ct));

            // Assert
            Assert.Equal(
                "Only confirmed appointments can be cancelled",
                ex.Message);

            VerifyNeverSaved();
        }

        [Fact]
        public async Task CancelAppointmentAsync_WhenSlotAlreadyStarted_ThrowsException()
        {
            // Arrange
            var appointment = CreateAppointment();
            appointment.AppointmentSlot!.StartDateTimeUtc =
                DateTime.UtcNow.AddHours(-1);

            SetupAppointmentLookup(appointment);
            SetupTokenVerification(true);

            // Act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.CancelAppointmentAsync(
                    appointment.AppointmentId,
                    "valid-token",
                    _ct));

            // Assert
            Assert.Equal(
                "Cannot cancel past or started appointments",
                ex.Message);

            VerifyNeverSaved();
        }

        [Fact]
        public async Task CancelAppointmentAsync_WhenTokenHashMissing_ThrowsUnauthorized()
        {
            // Arrange
            var appointment = CreateAppointment();
            appointment.ManagementTokenHash = string.Empty;

            SetupAppointmentLookup(appointment);

            // Act & Assert
            await Assert.ThrowsAsync<
                UnauthorizedAppointmentAccessException>(
                () => _sut.CancelAppointmentAsync(
                    appointment.AppointmentId,
                    "valid-token",
                    _ct));

            _tokenService.Verify(
                x => x.Verify(
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Never);

            VerifyNeverSaved();
        }

        #endregion

        #region Test Helpers

        private AppointmentSlot SetupValidBooking(
            bool branchActive = true,
            bool serviceActive = true)
        {
            var slot = CreateSlot();

            var branch = CreateBranch();
            branch.IsActive = branchActive;

            var service = CreateService();
            service.IsActive = serviceActive;

            slot.BranchService!.BranchId = branch.BranchId;
            slot.BranchService.ServiceId = service.ServiceId;

            _slotRepository
                .Setup(x => x.GetByIdAsync(slot.AppointmentSlotId, _ct))
                .ReturnsAsync(slot);

            _branchRepository
                .Setup(x => x.GetByIdAsync(branch.BranchId, _ct))
                .ReturnsAsync(branch);

            _serviceRepository
                .Setup(x => x.GetByIdAsync(service.ServiceId, _ct))
                .ReturnsAsync(service);

            _appointmentRepository
                .Setup(x => x.IsSlotOccupiedAsync(
                    slot.AppointmentSlotId, _ct))
                .ReturnsAsync(false);

            return slot;
        }

        private void SetupAppointmentLookup(Appointment appointment)
        {
            _appointmentRepository
                .Setup(x => x.GetByIdAsync(appointment.AppointmentId, _ct))
                .ReturnsAsync(appointment);
        }

        private void SetupToken()
        {
            _tokenService
                .Setup(x => x.GenerateToken())
                .Returns("test-management-token");

            _tokenService
                .Setup(x => x.HashToken("test-management-token"))
                .Returns("hashed-token");
        }

        private void SetupTokenVerification(bool valid)
        {
            _tokenService
                .Setup(x => x.Verify(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(valid);
        }

        private void VerifySavedOnce()
        {
            _unitOfWork.Verify(
                x => x.SaveChangesAsync(_ct),
                Times.Once);
        }

        private void VerifyNeverSaved()
        {
            _unitOfWork.Verify(
                x => x.SaveChangesAsync(_ct),
                Times.Never);
        }

        private static BranchEntity CreateBranch()
        {
            return new BranchEntity
            {
                BranchId = Guid.NewGuid(),
                BranchName = "Johannesburg Branch",
                BranchCode = "JHB",
                Address = "123 Main Road",
                City = "Johannesburg",
                Province = "Gauteng",
                ContactNumber = "0111234567",
                TimeZoneId = "South Africa Standard Time",
                IsActive = true
            };
        }

        private static ServiceEntity CreateService()
        {
            return new ServiceEntity
            {
                ServiceId = Guid.NewGuid(),
                ServiceName = "Haircut",
                DurationMinutes = 30,
                IsActive = true
            };
        }

        private static Customer CreateCustomer()
        {
            return new Customer
            {
                CustomerId = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Smith",
                EmailAddress = "customer@test.com",
                PhoneNumber = "0821234567"
            };
        }

        private static AppointmentSlot CreateSlot()
        {
            var branch = CreateBranch();
            var service = CreateService();

            var branchService = new BranchServiceEntity
            {
                BranchServiceId = Guid.NewGuid(),
                BranchId = branch.BranchId,
                ServiceId = service.ServiceId,
                Branch = branch,
                Service = service,
                Price = 150m,
                IsActive = true
            };

            return new AppointmentSlot
            {
                AppointmentSlotId = Guid.NewGuid(),
                BranchServiceId = branchService.BranchServiceId,
                BranchService = branchService,
                StartDateTimeUtc = DateTime.UtcNow.AddDays(2),
                EndDateTimeUtc = DateTime.UtcNow.AddDays(2).AddMinutes(30),
                IsActive = true
            };
        }

        private static Appointment CreateAppointment()
        {
            var slot = CreateSlot();
            var customer = CreateCustomer();

            return new Appointment
            {
                AppointmentId = Guid.NewGuid(),
                AppointmentReference = "APT-12345678",
                AppointmentSlotId = slot.AppointmentSlotId,
                AppointmentSlot = slot,
                CustomerId = customer.CustomerId,
                Customer = customer,
                Status = AppointmentStatus.Confirmed,
                ManagementTokenHash = "hashed-token",
                CreatedDate = DateTime.UtcNow.AddDays(-1)
            };
        }

        private static CreateAppointmentRequest CreateRequest(
            AppointmentSlot? slot = null)
        {
            slot ??= CreateSlot();

            // Deserialization avoids assuming the name of
            // the nested Customer request DTO class.
            var json = JsonSerializer.Serialize(new
            {
                AppointmentSlotId = slot.AppointmentSlotId,
                BranchServiceId = slot.BranchServiceId,
                Notes = "First appointment",
                Customer = new
                {
                    FirstName = "John",
                    LastName = "Smith",
                    EmailAddress = "customer@test.com",
                    PhoneNumber = "0821234567"
                }
            });

            return JsonSerializer.Deserialize<CreateAppointmentRequest>(json)!;
        }

        #endregion
    }
}