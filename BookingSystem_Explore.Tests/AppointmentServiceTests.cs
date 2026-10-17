using BookingSystem_Explore.BusinessLibrary.DTOs.Requests;
using BookingSystem_Explore.BusinessLibrary.Repositories;
using BookingSystem_Explore.BusinessLibrary.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BookingSystem_Explore.Tests
{
    public class AppointmentServiceTests
    {
        [Fact]
        public async Task BookAppointment_ReturnsResponse_WithManagementToken()
        {
            // Arrange - create in-memory store and repositories
            var store = new InMemoryDataStore();
            var appointmentRepo = new InMemoryAppointmentRepository(store);
            var slotRepo = new InMemoryAppointmentSlotRepository(store);
            var branchRepo = new InMemoryBranchRepository(store);
            var serviceRepo = new InMemoryServiceRepository(store);
            var customerRepo = new InMemoryCustomerRepository(store);
            var notificationRepo = new InMemoryNotificationRepository(store);
            var uow = new InMemoryUnitOfWork();
            var tokenService = new BookingSystem_Explore.Infrastructure.Security.TokenService();
            var logger = new NullLogger<AppointmentService>();

            var svc = new AppointmentService(
                appointmentRepo,
                slotRepo,
                branchRepo,
                serviceRepo,
                customerRepo,
                notificationRepo,
                uow,
                tokenService,
                logger
            );

            var slot = store.Slots.First();
            var branchService = store.BranchServices.First();

            var request = new CreateAppointmentRequest
            {
                BranchServiceId = branchService.BranchServiceId,
                AppointmentSlotId = slot.AppointmentSlotId,
                Customer = new CreateAppointmentRequest.CustomerDto
                {
                    FirstName = "Test",
                    LastName = "User",
                    EmailAddress = "test@example.com",
                    PhoneNumber = "1234567890"
                },
                Notes = "Unit test booking"
            };

            // Act
            var response = await svc.BookAppointmentAsync(request, CancellationToken.None);

            // Assert
            Assert.NotNull(response);
            Assert.False(string.IsNullOrWhiteSpace(response.ManagementToken));
            Assert.Equal(response.StartDateTimeUtc, slot.StartDateTimeUtc);
        }

        [Fact]
        public async Task GetAppointment_WithValidToken_ReturnsAppointment()
        {
            // Arrange
            var store = new InMemoryDataStore();
            var appointmentRepo = new InMemoryAppointmentRepository(store);
            var slotRepo = new InMemoryAppointmentSlotRepository(store);
            var branchRepo = new InMemoryBranchRepository(store);
            var serviceRepo = new InMemoryServiceRepository(store);
            var customerRepo = new InMemoryCustomerRepository(store);
            var notificationRepo = new InMemoryNotificationRepository(store);
            var uow = new InMemoryUnitOfWork();
            var tokenService = new BookingSystem_Explore.Infrastructure.Security.TokenService();
            var logger = new NullLogger<AppointmentService>();

            var svc = new AppointmentService(
                appointmentRepo,
                slotRepo,
                branchRepo,
                serviceRepo,
                customerRepo,
                notificationRepo,
                uow,
                tokenService,
                logger
            );

            var slot = store.Slots.First();
            var branchService = store.BranchServices.First();

            var request = new CreateAppointmentRequest
            {
                BranchServiceId = branchService.BranchServiceId,
                AppointmentSlotId = slot.AppointmentSlotId,
                Customer = new CreateAppointmentRequest.CustomerDto
                {
                    FirstName = "Test",
                    LastName = "User",
                    EmailAddress = "test@example.com",
                    PhoneNumber = "1234567890"
                },
                Notes = "Unit test booking"
            };

            var created = await svc.BookAppointmentAsync(request, CancellationToken.None);

            // Act
            var appointment = await svc.GetAppointmentAsync(created.AppointmentId, created.ManagementToken, CancellationToken.None);

            // Assert
            Assert.NotNull(appointment);
            Assert.Equal(created.AppointmentId, appointment!.AppointmentId);
            Assert.Equal(created.AppointmentReference, appointment.AppointmentReference);
        }

        [Fact]
        public async Task CancelAppointment_WithValidToken_UpdatesStatus()
        {
            // Arrange
            var store = new InMemoryDataStore();
            var appointmentRepo = new InMemoryAppointmentRepository(store);
            var slotRepo = new InMemoryAppointmentSlotRepository(store);
            var branchRepo = new InMemoryBranchRepository(store);
            var serviceRepo = new InMemoryServiceRepository(store);
            var customerRepo = new InMemoryCustomerRepository(store);
            var notificationRepo = new InMemoryNotificationRepository(store);
            var uow = new InMemoryUnitOfWork();
            var tokenService = new BookingSystem_Explore.Infrastructure.Security.TokenService();
            var logger = new NullLogger<AppointmentService>();

            var svc = new AppointmentService(
                appointmentRepo,
                slotRepo,
                branchRepo,
                serviceRepo,
                customerRepo,
                notificationRepo,
                uow,
                tokenService,
                logger
            );

            var slot = store.Slots.First();
            var branchService = store.BranchServices.First();

            var request = new CreateAppointmentRequest
            {
                BranchServiceId = branchService.BranchServiceId,
                AppointmentSlotId = slot.AppointmentSlotId,
                Customer = new CreateAppointmentRequest.CustomerDto
                {
                    FirstName = "Test",
                    LastName = "User",
                    EmailAddress = "test@example.com",
                    PhoneNumber = "1234567890"
                },
                Notes = "Unit test booking"
            };

            var created = await svc.BookAppointmentAsync(request, CancellationToken.None);

            // Act
            var cancelled = await svc.CancelAppointmentAsync(created.AppointmentId, created.ManagementToken, CancellationToken.None);

            // Assert
            Assert.Equal("Cancelled", cancelled.Status);
        }
    }
}
