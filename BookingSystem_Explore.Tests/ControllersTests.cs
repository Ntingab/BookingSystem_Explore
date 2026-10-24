using BookingSystem_Explore.Api.Controllers;
using BookingSystem_Explore.BusinessLibrary.DTOs.Requests;
using BookingSystem_Explore.BusinessLibrary.DTOs.Responses;
using BookingSystem_Explore.BusinessLibrary.Repositories;
using BookingSystem_Explore.BusinessLibrary.Services;
using BookingSystem_Explore.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace BookingSystem_Explore.Tests
{
    public class ControllersTests
    {
        [Fact]
        public async Task BranchesController_GetBranches_ReturnsList()
        {
            var store = new InMemoryDataStore();
            var branchRepo = new InMemoryBranchRepository(store);
            var branchService = new BranchService(branchRepo, new InMemoryUnitOfWork());
            var ctrl = new BranchesController(branchService);

            var result = await ctrl.GetBranchesAsync(CancellationToken.None);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsAssignableFrom<IReadOnlyList<BranchResponse>>(ok.Value);
            Assert.NotEmpty(list);
        }

        [Fact]
        public async Task BranchesController_AddAndUpdateBranchAndService_Works()
        {
            var store = new InMemoryDataStore();
            var branchService = new BranchService(new InMemoryBranchRepository(store), new InMemoryUnitOfWork());
            var controller = new BranchesController(branchService)
            {
                ControllerContext = new ControllerContext { HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext() }
            };

            var createdResult = await controller.AddBranchAsync(new CreateBranchRequest
            {
                BranchCode = "TEST",
                BranchName = "Test Branch",
                Address = "1 Test Street",
                City = "Johannesburg",
                Province = "Gauteng",
                ContactNumber = "0110000000",
                TimeZoneId = "Africa/Johannesburg"
            }, CancellationToken.None);
            var created = Assert.IsType<BranchResponse>(Assert.IsType<CreatedAtActionResult>(createdResult.Result).Value);

            var updatedResult = await controller.UpdateBranchAsync(created.BranchId, new UpdateBranchRequest
            {
                BranchCode = "TEST",
                BranchName = "Updated Test Branch",
                Address = "2 Test Street",
                City = "Johannesburg",
                Province = "Gauteng",
                ContactNumber = "0110000001",
                TimeZoneId = "Africa/Johannesburg",
                IsActive = true
            }, CancellationToken.None);
            var updated = Assert.IsType<BranchResponse>(Assert.IsType<OkObjectResult>(updatedResult.Result).Value);
            Assert.Equal("Updated Test Branch", updated.BranchName);

            var serviceResult = await controller.AddBranchServiceAsync(created.BranchId, new CreateBranchServiceRequest
            {
                ServiceId = store.Services[0].ServiceId,
                Price = 150m
            }, CancellationToken.None);
            var branchServiceResponse = Assert.IsType<BranchServiceResponse>(Assert.IsType<CreatedAtActionResult>(serviceResult.Result).Value);

            var updatedServiceResult = await controller.UpdateBranchServiceAsync(created.BranchId, branchServiceResponse.BranchServiceId, new UpdateBranchServiceRequest
            {
                Price = 175m,
                IsActive = true
            }, CancellationToken.None);
            var updatedService = Assert.IsType<BranchServiceResponse>(Assert.IsType<OkObjectResult>(updatedServiceResult.Result).Value);
            Assert.Equal(175m, updatedService.Price);
        }

        [Fact]
        public async Task ServicesController_GetServices_ReturnsDistinct()
        {
            var store = new InMemoryDataStore();
            var serviceRepo = new InMemoryServiceRepository(store);
            var serviceService = new BookingSystem_Explore.BusinessLibrary.Services.ServiceService(serviceRepo, new InMemoryUnitOfWork());

            var ctrl = new ServicesController(serviceService);

            var res = await ctrl.GetServicesAsync(CancellationToken.None);
            var ok = Assert.IsType<OkObjectResult>(res.Result);
            var list = Assert.IsAssignableFrom<IReadOnlyList<ServiceResponse>>(ok.Value);
            Assert.NotEmpty(list);

            // test get by id
            var first = list.First();
            var byId = await ctrl.GetServiceByIdAsync(first.ServiceId, CancellationToken.None);
            var ok2 = Assert.IsType<OkObjectResult>(byId.Result);
            var svc = Assert.IsType<ServiceResponse>(ok2.Value);
            Assert.Equal(first.ServiceId, svc.ServiceId);
        }

        [Fact]
        public async Task AppointmentSlotsController_GetAvailableAndById_ReturnsSlots()
        {
            var store = new InMemoryDataStore();
            var slotRepo = new InMemoryAppointmentSlotRepository(store);
            var slotService = new AppointmentSlotService(slotRepo, new InMemoryUnitOfWork());

            var options = new DbContextOptionsBuilder<AppointmentBookingDbContext>()
                .UseInMemoryDatabase("slots_db").Options;
            await using var db = new AppointmentBookingDbContext(options);

            // seed slots
            foreach (var s in store.Slots)
            {
                db.AppointmentSlots.Add(s);
            }
            await db.SaveChangesAsync();

            var ctrl = new AppointmentSlotsController(slotService);

            var branchServiceId = store.BranchServices.First().BranchServiceId;
            var date = DateOnly.FromDateTime(store.Slots.First().StartDateTimeUtc);

            var res = await ctrl.GetAvailableSlotsAsync(branchServiceId, date.ToString(), CancellationToken.None);
            var ok = Assert.IsType<OkObjectResult>(res.Result);
            var list = Assert.IsAssignableFrom<IReadOnlyList<AppointmentSlotResponse>>(ok.Value);
            Assert.NotEmpty(list);

            // by id
            var id = list.First().AppointmentSlotId;
            var byId = await ctrl.GetSlotByIdAsync(id, CancellationToken.None);
            var ok2 = Assert.IsType<OkObjectResult>(byId.Result);
            var slot = Assert.IsType<AppointmentSlotResponse>(ok2.Value);
            Assert.Equal(id, slot.AppointmentSlotId);
        }

        [Fact]
        public async Task AppointmentsController_BookAndGetAndCancel_Works()
        {
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
            var logger = new NullLogger<BookingSystem_Explore.BusinessLibrary.Services.AppointmentService>();

            var svc = new BookingSystem_Explore.BusinessLibrary.Services.AppointmentService(
                appointmentRepo, slotRepo, branchRepo, serviceRepo, customerRepo, notificationRepo, uow, tokenService, phoneEncryptionService, logger
            );

            var controller = new AppointmentsController(svc);
            // ensure controller has HttpContext for header-based token
            controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext { HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext() };

            var branchService = store.BranchServices.First();
            var slot = store.Slots.FirstOrDefault(s => s.StartDateTimeUtc > DateTime.UtcNow);
            if (slot == null)
            {
                var svcEntity = store.Services.First();
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
                Customer = new CreateAppointmentRequest.CustomerDto { FirstName = "T", LastName = "U", EmailAddress = "a@b.com", PhoneNumber = phoneEncryptionService.Encrypt("123") },
                Notes = "test"
            };

            var createResult = await controller.BookAppointmentAsync(request, CancellationToken.None);
            var created = Assert.IsType<CreatedAtRouteResult>(createResult.Result);
            var payload = Assert.IsType<AppointmentCreatedResponse>(created.Value);
            Assert.False(string.IsNullOrWhiteSpace(payload.ManagementToken));

            // get by id
            // set management token header for subsequent retrieval and cancellation
            controller.Request.Headers["X-Management-Token"] = payload.ManagementToken;

            var getRes = await controller.GetAppointmentByIdAsync(payload.AppointmentId, CancellationToken.None);
            var ok = Assert.IsType<OkObjectResult>(getRes.Result);
            var appt = Assert.IsType<BookingSystem_Explore.BusinessLibrary.DTOs.Responses.AppointmentResponse>(ok.Value);
            Assert.Equal(payload.AppointmentId, appt.AppointmentId);

            // cancel
            var cancelRes = await controller.CancelAppointmentAsync(payload.AppointmentId, CancellationToken.None);
            var okCancel = Assert.IsType<OkObjectResult>(cancelRes.Result);
            var cancelled = Assert.IsType<BookingSystem_Explore.BusinessLibrary.DTOs.Responses.AppointmentResponse>(okCancel.Value!);
            Assert.Equal("Cancelled", cancelled.Status);
        }

        [Fact]
        public async Task NotificationsController_ReturnsNotifications()
        {
            var store = new InMemoryDataStore();
            var appointmentId = Guid.NewGuid();

            var tokenService = new BookingSystem_Explore.Infrastructure.Security.TokenService();
            var token = tokenService.GenerateToken();
            var hash = tokenService.HashToken(token);

            // create appointment and notification in in-memory repositories
            var appointmentRepo = new InMemoryAppointmentRepository(store);
            var notificationRepo = new InMemoryNotificationRepository(store);
            // add appointment with token hash
            store.Appointments.Add(new BookingSystem_Explore.Domain.Entities.Appointment { AppointmentId = appointmentId, AppointmentReference = "R1", ManagementTokenHash = hash, CreatedDate = DateTime.UtcNow, Status = BookingSystem_Explore.Domain.Enums.AppointmentStatus.Confirmed });
            store.Notifications.Add(new BookingSystem_Explore.Domain.Entities.Notification { NotificationId = Guid.NewGuid(), AppointmentId = appointmentId, Channel = BookingSystem_Explore.Domain.Enums.NotificationChannel.Email, Message = "m", Recipient = "r", Status = BookingSystem_Explore.Domain.Enums.NotificationStatus.SimulatedSent, CreatedDate = DateTime.UtcNow });

            var notificationService = new BookingSystem_Explore.BusinessLibrary.Services.NotificationService(appointmentRepo, notificationRepo, tokenService);
            var ctrl = new NotificationsController(notificationService);
            ctrl.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext { HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext() };
            ctrl.Request.Headers["X-Management-Token"] = token;

            var res = await ctrl.GetAppointmentNotificationsAsync(appointmentId, CancellationToken.None);
            var ok = Assert.IsType<OkObjectResult>(res.Result);
            var list = Assert.IsAssignableFrom<IEnumerable<object>>(ok.Value);
            Assert.NotEmpty(list);
        }
    }
}
