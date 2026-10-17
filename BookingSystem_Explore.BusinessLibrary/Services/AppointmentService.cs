using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using BookingSystem_Explore.BusinessLibrary.DTOs.Requests;
using BookingSystem_Explore.BusinessLibrary.DTOs.Responses;
using BookingSystem_Explore.BusinessLibrary.Interfaces.Repositories;
using BookingSystem_Explore.BusinessLibrary.Interfaces.Services;
using BookingSystem_Explore.Infrastructure.Security;
using BookingSystem_Explore.Infrastructure.Entities;

namespace BookingSystem_Explore.BusinessLibrary.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IAppointmentSlotRepository _slotRepository;
        private readonly IBranchRepository _branchRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly BookingSystem_Explore.BusinessLibrary.Interfaces.IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AppointmentService> _logger;

        public AppointmentService(
            IAppointmentRepository appointmentRepository,
            IAppointmentSlotRepository slotRepository,
            IBranchRepository branchRepository,
            IServiceRepository serviceRepository,
            ICustomerRepository customerRepository,
            INotificationRepository notificationRepository,
            BookingSystem_Explore.BusinessLibrary.Interfaces.IUnitOfWork unitOfWork,
            ITokenService tokenService,
            ILogger<AppointmentService> logger)
        {
            _appointmentRepository = appointmentRepository;
            _slotRepository = slotRepository;
            _branchRepository = branchRepository;
            _serviceRepository = serviceRepository;
            _customerRepository = customerRepository;
            _notificationRepository = notificationRepository;
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<AppointmentResponse> BookAppointmentAsync(CreateAppointmentRequest request, CancellationToken cancellationToken)
        {
            // Basic flow: validate existence and availability
            var slot = await _slotRepository.GetByIdAsync(request.AppointmentSlotId, cancellationToken);
            if (slot == null) throw new InvalidOperationException("Slot not found");

            var branchService = slot.BranchService ?? throw new InvalidOperationException("Slot missing branch-service info");
            var branch = await _branchRepository.GetByIdAsync(branchService.BranchId, cancellationToken);
            if (branch == null || !branch.IsActive) throw new InvalidOperationException("Branch not active or not found");

            var service = await _serviceRepository.GetByIdAsync(branchService.ServiceId, cancellationToken);
            if (service == null || !service.IsActive) throw new InvalidOperationException("Service not active or not found");

            if (!slot.IsActive || slot.StartDateTimeUtc <= DateTime.UtcNow) throw new InvalidOperationException("Slot not available");

            var occupied = await _appointmentRepository.IsSlotOccupiedAsync(slot.AppointmentSlotId, cancellationToken);
            if (occupied) throw new InvalidOperationException("Slot already occupied");

            // create customer
            var customer = new Customer
            {
                CustomerId = Guid.NewGuid(),
                FirstName = request.Customer.FirstName,
                LastName = request.Customer.LastName,
                EmailAddress = request.Customer.EmailAddress,
                PhoneNumber = request.Customer.PhoneNumber,
                CreatedDate = DateTime.UtcNow
            };
            await _customerRepository.AddAsync(customer, cancellationToken);

            // create appointment and token
            var appointment = new Appointment
            {
                AppointmentId = Guid.NewGuid(),
                AppointmentReference = $"APT-{Guid.NewGuid().ToString().Substring(0,8).ToUpper()}",
                CustomerId = customer.CustomerId,
                AppointmentSlotId = slot.AppointmentSlotId,
                Status = BookingSystem_Explore.Infrastructure.Enums.AppointmentStatus.Confirmed,
                CreatedDate = DateTime.UtcNow,
                Notes = request.Notes
            };

            // generate management token and hash
            var token = _tokenService.GenerateToken();
            var tokenHash = _tokenService.HashToken(token);

            // store tokenHash in Appointment - add property? Instead, store as Notification error-free placeholder: For now attach via a custom field not present.
            // To keep model stable, we will create a Pending notification containing tokenHash in ErrorMessage (non-sensitive storage). In real system, add dedicated column.

            var notification = new Notification
            {
                NotificationId = Guid.NewGuid(),
                AppointmentId = appointment.AppointmentId,
                NotificationType = BookingSystem_Explore.Infrastructure.Enums.NotificationType.BookingConfirmation,
                Channel = BookingSystem_Explore.Infrastructure.Enums.NotificationChannel.Email,
                Recipient = customer.EmailAddress,
                Message = "Pending confirmation",
                Status = BookingSystem_Explore.Infrastructure.Enums.NotificationStatus.Pending,
                CreatedDate = DateTime.UtcNow,
                ErrorMessage = tokenHash
            };

            await _appointmentRepository.AddAsync(appointment, cancellationToken);
            await _notificationRepository.AddAsync(notification, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Appointment {AppointmentId} created successfully", appointment.AppointmentId);

            // return response with management token
            return new AppointmentResponse
            {
                AppointmentId = appointment.AppointmentId,
                AppointmentReference = appointment.AppointmentReference,
                Status = "Confirmed",
                BranchName = branch.BranchName,
                ServiceName = service.ServiceName,
                StartDateTimeUtc = slot.StartDateTimeUtc,
                EndDateTimeUtc = slot.EndDateTimeUtc,
                NotificationStatus = "Pending"
                // management token must be returned only on creation - here we don't include it in response object model; you'd include it in a specialized CreatedResponse.
            };
        }

        public Task<AppointmentResponse?> GetAppointmentAsync(Guid appointmentId, string managementToken, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<AppointmentResponse?> GetAppointmentByReferenceAsync(string reference, string managementToken, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<AppointmentResponse> CancelAppointmentAsync(Guid appointmentId, string managementToken, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
