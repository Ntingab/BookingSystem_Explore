using BookingSystem_Explore.BusinessLibrary.DTOs.Responses;
using BookingSystem_Explore.BusinessLibrary.Interfaces.Services;
using BookingSystem_Explore.Domain.Interfaces.Repositories;

namespace BookingSystem_Explore.BusinessLibrary.Services
{
    public class AppointmentSlotService : IAppointmentSlotService
    {
        private readonly IAppointmentSlotRepository _slotRepository;
        private readonly BookingSystem_Explore.Domain.Interfaces.IUnitOfWork _unitOfWork;

        public AppointmentSlotService(IAppointmentSlotRepository slotRepository, BookingSystem_Explore.Domain.Interfaces.IUnitOfWork unitOfWork)
        {
            _slotRepository = slotRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<IReadOnlyList<AppointmentSlotResponse>> GetAllAvailableSlotsAsync(CancellationToken cancellationToken)
        {
            var slots = await _slotRepository.GetAllAvailableSlotsAsync(cancellationToken);
            return slots.Select(ToResponse).ToList();
        }

        public async Task<IReadOnlyList<AppointmentSlotResponse>> GetAvailableSlotsAsync(Guid branchServiceId, DateOnly date, CancellationToken cancellationToken)
        {
            var slots = await _slotRepository.GetAvailableSlotsAsync(branchServiceId, date, cancellationToken);
            return slots.Select(ToResponse).ToList();
        }

        public async Task<AppointmentSlotResponse?> GetSlotByIdAsync(Guid slotId, CancellationToken cancellationToken)
        {
            var s = await _slotRepository.GetByIdAsync(slotId, cancellationToken);
            if (s == null) return null;
            return ToResponse(s);
        }

        public async Task<AppointmentSlotResponse> CreateSlotAsync(BookingSystem_Explore.BusinessLibrary.DTOs.Requests.CreateAppointmentSlotRequest request, CancellationToken cancellationToken)
        {
            // Normalize incoming times to UTC
            var startUtc = request.StartDateTimeUtc.Kind == DateTimeKind.Utc
                ? request.StartDateTimeUtc
                : request.StartDateTimeUtc.ToUniversalTime();

            var endUtc = request.EndDateTimeUtc.Kind == DateTimeKind.Utc
                ? request.EndDateTimeUtc
                : request.EndDateTimeUtc.ToUniversalTime();

            // Validate date range: database has CHECK (EndDateTimeUtc > StartDateTimeUtc)
            if (endUtc <= startUtc)
            {
                throw new ArgumentException("EndDateTimeUtc must be greater than StartDateTimeUtc");
            }

            // Ensure unique (BranchServiceId, StartDateTimeUtc) to avoid UQ constraint violation
            var existingForDay = await _slotRepository.GetAvailableSlotsAsync(request.BranchServiceId, DateOnly.FromDateTime(startUtc), cancellationToken);
            if (existingForDay.Any(s => s.StartDateTimeUtc == startUtc && s.BranchServiceId == request.BranchServiceId))
            {
                throw new InvalidOperationException("An appointment slot with the same start time already exists for this branch service.");
            }

            var slot = new BookingSystem_Explore.Domain.Entities.AppointmentSlot
            {
                AppointmentSlotId = Guid.NewGuid(),
                BranchServiceId = request.BranchServiceId,
                StartDateTimeUtc = startUtc,
                EndDateTimeUtc = endUtc,
                IsActive = request.IsActive,
                CreatedDate = DateTime.UtcNow
            };

            await _slotRepository.AddAsync(slot, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new AppointmentSlotResponse
            {
                AppointmentSlotId = slot.AppointmentSlotId,
                StartDateTimeUtc = slot.StartDateTimeUtc,
                EndDateTimeUtc = slot.EndDateTimeUtc
            };
        }

        public async Task<AppointmentSlotResponse?> UpdateSlotAsync(Guid slotId, BookingSystem_Explore.BusinessLibrary.DTOs.Requests.UpdateAppointmentSlotRequest request, CancellationToken cancellationToken)
        {
            var existing = await _slotRepository.GetByIdAsync(slotId, cancellationToken);
            if (existing == null) return null;

            existing.StartDateTimeUtc = request.StartDateTimeUtc;
            existing.EndDateTimeUtc = request.EndDateTimeUtc;
            existing.IsActive = request.IsActive;
            existing.UpdatedDate = DateTime.UtcNow;

            await _slotRepository.UpdateAsync(existing, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new AppointmentSlotResponse
            {
                AppointmentSlotId = existing.AppointmentSlotId,
                StartDateTimeUtc = existing.StartDateTimeUtc,
                EndDateTimeUtc = existing.EndDateTimeUtc
            };
        }

        private static AppointmentSlotResponse ToResponse(BookingSystem_Explore.Domain.Entities.AppointmentSlot slot)
        {
            return new AppointmentSlotResponse
            {
                AppointmentSlotId = slot.AppointmentSlotId,
                StartDateTimeUtc = slot.StartDateTimeUtc,
                EndDateTimeUtc = slot.EndDateTimeUtc
            };
        }
    }
}
