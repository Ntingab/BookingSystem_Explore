using BookingSystem_Explore.BusinessLibrary.DTOs.Requests;
using BookingSystem_Explore.BusinessLibrary.DTOs.Responses;
using BookingSystem_Explore.BusinessLibrary.Interfaces.Services;
using BookingSystem_Explore.Domain.Entities;
using BookingSystem_Explore.Domain.Interfaces.Repositories;
using BookingSystem_Explore.Domain.Security;
using Microsoft.Extensions.Logging;

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
        private readonly BookingSystem_Explore.BusinessLibrary.Notifications.SimulatedNotificationProcessor? _simulatedNotificationProcessor;
        private readonly BookingSystem_Explore.Domain.Interfaces.IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly IPhoneEncryptionService _phoneEncryptionService;
        private readonly ILogger<AppointmentService> _logger;

        public AppointmentService(
            IAppointmentRepository appointmentRepository,
            IAppointmentSlotRepository slotRepository,
            IBranchRepository branchRepository,
            IServiceRepository serviceRepository,
            ICustomerRepository customerRepository,
            INotificationRepository notificationRepository,
            BookingSystem_Explore.Domain.Interfaces.IUnitOfWork unitOfWork,
            ITokenService tokenService,
            IPhoneEncryptionService phoneEncryptionService,
            ILogger<AppointmentService> logger)
        {
            _appointmentRepository = appointmentRepository;
            _slotRepository = slotRepository;
            _branchRepository = branchRepository;
            _serviceRepository = serviceRepository;
            _customerRepository = customerRepository;
            _notificationRepository = notificationRepository;
            _simulatedNotificationProcessor = null;
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _phoneEncryptionService = phoneEncryptionService;
            _logger = logger;
        }

        public async Task<AppointmentCreatedResponse> BookAppointmentAsync(CreateAppointmentRequest request, CancellationToken cancellationToken)
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

            var phoneNumber = _phoneEncryptionService.TryDecrypt(request.Customer.PhoneNumber, out var decryptedPhone)
                ? decryptedPhone
                : request.Customer.PhoneNumber;

            // create customer
            var customer = new Customer
            {
                CustomerId = Guid.NewGuid(),
                FirstName = request.Customer.FirstName,
                LastName = request.Customer.LastName,
                EmailAddress = request.Customer.EmailAddress,
                PhoneNumber = _phoneEncryptionService.Encrypt(phoneNumber),
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
                Status = BookingSystem_Explore.Domain.Enums.AppointmentStatus.Confirmed,
                CreatedDate = DateTime.UtcNow,
                Notes = request.Notes
            };

            // generate management token and hash
            var token = _tokenService.GenerateToken();
            var tokenHash = _tokenService.HashToken(token);

            // store tokenHash in appointment (ManagementTokenHash) so we can verify later
            appointment.ManagementTokenHash = tokenHash;

            var notification = new Notification
            {
                NotificationId = Guid.NewGuid(),
                AppointmentId = appointment.AppointmentId,
                NotificationType = BookingSystem_Explore.Domain.Enums.NotificationType.BookingConfirmation,
                Channel = BookingSystem_Explore.Domain.Enums.NotificationChannel.Email,
                Recipient = customer.EmailAddress,
                Message = "Your appointment has been confirmed",
                Status = BookingSystem_Explore.Domain.Enums.NotificationStatus.Pending,
                CreatedDate = DateTime.UtcNow,
                ErrorMessage = null
            };

            await _appointmentRepository.AddAsync(appointment, cancellationToken);
            await _notificationRepository.AddAsync(notification, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Appointment {AppointmentId} created successfully", appointment.AppointmentId);

            // return creation response with management token (token returned only once)
            return new AppointmentCreatedResponse
            {
                AppointmentId = appointment.AppointmentId,
                AppointmentReference = appointment.AppointmentReference,
                Status = "Confirmed",
                BranchName = branch.BranchName,
                ServiceName = service.ServiceName,
                StartDateTimeUtc = slot.StartDateTimeUtc,
                EndDateTimeUtc = slot.EndDateTimeUtc,
                NotificationStatus = "Pending",
                ManagementToken = token
            };
        }

        public async Task<AppointmentResponse?> GetAppointmentAsync(Guid appointmentId, string managementToken, CancellationToken cancellationToken)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId, cancellationToken);
            if (appointment == null) return null;

            // verify management token using stored token hash on appointment
            var tokenHash = appointment.ManagementTokenHash;
            var isValidToken = _tokenService.Verify(managementToken, tokenHash);

            if (!isValidToken)
            {
                throw new BookingSystem_Explore.BusinessLibrary.Exceptions.UnauthorizedAppointmentAccessException("Invalid management token");
            }

            return new AppointmentResponse
            {
                AppointmentId = appointment.AppointmentId,
                AppointmentReference = appointment.AppointmentReference,
                Status = appointment.Status.ToString(),
                BranchName = appointment.AppointmentSlot?.BranchService?.Branch?.BranchName ?? string.Empty,
                ServiceName = appointment.AppointmentSlot?.BranchService?.Service?.ServiceName ?? string.Empty,
                StartDateTimeUtc = appointment.AppointmentSlot?.StartDateTimeUtc ?? default,
                EndDateTimeUtc = appointment.AppointmentSlot?.EndDateTimeUtc ?? default,
                NotificationStatus = appointment.Notifications?.OrderByDescending(n => n.CreatedDate).FirstOrDefault()?.Status.ToString() ?? string.Empty
            };
        }

        public async Task<AppointmentResponse?> GetAppointmentByReferenceAsync(string reference, string managementToken, CancellationToken cancellationToken)
        {
            var appointment = await _appointmentRepository.GetByReferenceAsync(reference, cancellationToken);
            if (appointment == null) return null;
            // reuse GetAppointmentAsync logic by verifying token
            return await GetAppointmentAsync(appointment.AppointmentId, managementToken, cancellationToken);
        }

        public async Task<AppointmentResponse> CancelAppointmentAsync(Guid appointmentId, string managementToken, CancellationToken cancellationToken)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId, cancellationToken);
            if (appointment == null) throw new BookingSystem_Explore.BusinessLibrary.Exceptions.NotFoundException("Appointment not found");

            var tokenHash = appointment.ManagementTokenHash;
            if (string.IsNullOrEmpty(tokenHash) || !_tokenService.Verify(managementToken, tokenHash))
            {
                throw new BookingSystem_Explore.BusinessLibrary.Exceptions.UnauthorizedAppointmentAccessException("Invalid management token");
            }

            if (appointment.Status != BookingSystem_Explore.Domain.Enums.AppointmentStatus.Confirmed)
            {
                throw new InvalidOperationException("Only confirmed appointments can be cancelled");
            }

            var slotStart = appointment.AppointmentSlot?.StartDateTimeUtc ?? DateTime.MinValue;
            if (slotStart <= DateTime.UtcNow)
            {
                throw new InvalidOperationException("Cannot cancel past or started appointments");
            }

            // perform cancellation
            appointment.Status = BookingSystem_Explore.Domain.Enums.AppointmentStatus.Cancelled;
            appointment.CancelledDate = DateTime.UtcNow;
            appointment.UpdatedDate = DateTime.UtcNow;

            // create pending cancellation notification
            var notification = new Notification
            {
                NotificationId = Guid.NewGuid(),
                AppointmentId = appointment.AppointmentId,
                NotificationType = BookingSystem_Explore.Domain.Enums.NotificationType.BookingCancellation,
                Channel = BookingSystem_Explore.Domain.Enums.NotificationChannel.Email,
                Recipient = appointment.Customer?.EmailAddress ?? string.Empty,
                Message = "Pending cancellation",
                Status = BookingSystem_Explore.Domain.Enums.NotificationStatus.Pending,
                CreatedDate = DateTime.UtcNow
            };

            await _notificationRepository.AddAsync(notification, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Appointment {AppointmentId} cancelled", appointment.AppointmentId);

            // process notification but do not rollback on failure
            try
            {
                // Prefer a DI-provided processor if available; otherwise create a processor with a compatible logger.
                var processor = _simulatedNotificationProcessor
                    ?? new BookingSystem_Explore.BusinessLibrary.Notifications.SimulatedNotificationProcessor(
                        _notificationRepository,
                        Microsoft.Extensions.Logging.Abstractions.NullLogger<BookingSystem_Explore.BusinessLibrary.Notifications.SimulatedNotificationProcessor>.Instance);

                await processor.ProcessNotificationAsync(notification, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process cancellation notification for {AppointmentId}", appointment.AppointmentId);
            }

            return new AppointmentResponse
            {
                AppointmentId = appointment.AppointmentId,
                AppointmentReference = appointment.AppointmentReference,
                Status = appointment.Status.ToString(),
                BranchName = appointment.AppointmentSlot?.BranchService?.Branch?.BranchName ?? string.Empty,
                ServiceName = appointment.AppointmentSlot?.BranchService?.Service?.ServiceName ?? string.Empty,
                StartDateTimeUtc = appointment.AppointmentSlot?.StartDateTimeUtc ?? default,
                EndDateTimeUtc = appointment.AppointmentSlot?.EndDateTimeUtc ?? default,
                NotificationStatus = notification.Status.ToString()
            };
        }
    }
}
