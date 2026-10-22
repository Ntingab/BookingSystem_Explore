using BookingSystem_Explore.BusinessLibrary.DTOs.Requests;
using BookingSystem_Explore.BusinessLibrary.Repositories;
using BookingSystem_Explore.BusinessLibrary.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
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
            var phoneEncryptionService = new BookingSystem_Explore.Infrastructure.Security.PhoneEncryptionService(
                Options.Create(new BookingSystem_Explore.Infrastructure.Security.PhoneEncryptionOptions { Key = "unit-test-phone-key" }));
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
                phoneEncryptionService,
                logger
            );

            var branchService = store.BranchServices.First();
            var slot = store.Slots.FirstOrDefault(s => s.StartDateTimeUtc > DateTime.UtcNow);
            if (slot == null)
            {
                // create a future slot if the seeded ones are in the past
                var svcEntity = store.Services.FirstOrDefault(s => s.ServiceId == branchService.ServiceId) ?? store.Services.First();
                var start = DateTime.UtcNow.AddHours(1);
                var end = start.AddMinutes(svcEntity.DurationMinutes);
                var newSlot = new BookingSystem_Explore.Domain.Entities.AppointmentSlot { AppointmentSlotId = Guid.NewGuid(), BranchServiceId = branchService.BranchServiceId, StartDateTimeUtc = start, EndDateTimeUtc = end, CreatedDate = DateTime.UtcNow };
                store.Slots.Add(newSlot);
                slot = newSlot;
            }

            var request = new CreateAppointmentRequest
            {
                BranchServiceId = branchService.BranchServiceId,
                AppointmentSlotId = slot.AppointmentSlotId,
                Customer = new CreateAppointmentRequest.CustomerDto
                {
                    FirstName = "Test",
                    LastName = "User",
                    EmailAddress = "test@example.com",
                    PhoneNumber = phoneEncryptionService.Encrypt("1234567890")
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
            var phoneEncryptionService = new BookingSystem_Explore.Infrastructure.Security.PhoneEncryptionService(
                Options.Create(new BookingSystem_Explore.Infrastructure.Security.PhoneEncryptionOptions { Key = "unit-test-phone-key" }));
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
                phoneEncryptionService,
                logger
            );

            var branchService = store.BranchServices.First();
            var slot = store.Slots.FirstOrDefault(s => s.StartDateTimeUtc > DateTime.UtcNow);
            if (slot == null)
            {
                var svcEntity = store.Services.FirstOrDefault(s => s.ServiceId == branchService.ServiceId) ?? store.Services.First();
                var start = DateTime.UtcNow.AddHours(1);
                var end = start.AddMinutes(svcEntity.DurationMinutes);
                var newSlot = new BookingSystem_Explore.Domain.Entities.AppointmentSlot { AppointmentSlotId = Guid.NewGuid(), BranchServiceId = branchService.BranchServiceId, StartDateTimeUtc = start, EndDateTimeUtc = end, CreatedDate = DateTime.UtcNow };
                store.Slots.Add(newSlot);
                slot = newSlot;
            }

            var request = new CreateAppointmentRequest
            {
                BranchServiceId = branchService.BranchServiceId,
                AppointmentSlotId = slot.AppointmentSlotId,
                Customer = new CreateAppointmentRequest.CustomerDto
                {
                    FirstName = "Test",
                    LastName = "User",
                    EmailAddress = "test@example.com",
                    PhoneNumber = phoneEncryptionService.Encrypt("1234567890")
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
            var phoneEncryptionService = new BookingSystem_Explore.Infrastructure.Security.PhoneEncryptionService(
                Options.Create(new BookingSystem_Explore.Infrastructure.Security.PhoneEncryptionOptions { Key = "unit-test-phone-key" }));
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
                phoneEncryptionService,
                logger
            );

            var branchService = store.BranchServices.First();
            var slot = store.Slots.FirstOrDefault(s => s.StartDateTimeUtc > DateTime.UtcNow);
            if (slot == null)
            {
                var svcEntity = store.Services.FirstOrDefault(s => s.ServiceId == branchService.ServiceId) ?? store.Services.First();
                var start = DateTime.UtcNow.AddHours(1);
                var end = start.AddMinutes(svcEntity.DurationMinutes);
                var newSlot = new BookingSystem_Explore.Domain.Entities.AppointmentSlot { AppointmentSlotId = Guid.NewGuid(), BranchServiceId = branchService.BranchServiceId, StartDateTimeUtc = start, EndDateTimeUtc = end, CreatedDate = DateTime.UtcNow };
                store.Slots.Add(newSlot);
                slot = newSlot;
            }

            var request = new CreateAppointmentRequest
            {
                BranchServiceId = branchService.BranchServiceId,
                AppointmentSlotId = slot.AppointmentSlotId,
                Customer = new CreateAppointmentRequest.CustomerDto
                {
                    FirstName = "Test",
                    LastName = "User",
                    EmailAddress = "test@example.com",
                    PhoneNumber = phoneEncryptionService.Encrypt("1234567890")
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
