using BookingSystem_Explore.BusinessLibrary.DTOs.Responses;
using BookingSystem_Explore.BusinessLibrary.Interfaces.Services;
using BookingSystem_Explore.Domain.Interfaces.Repositories;

namespace BookingSystem_Explore.BusinessLibrary.Services
{
    public class AppointmentSlotService : IAppointmentSlotService
    {
        private readonly IAppointmentSlotRepository _slotRepository;

        public AppointmentSlotService(IAppointmentSlotRepository slotRepository)
        {
            _slotRepository = slotRepository;
        }

        public async Task<IReadOnlyList<AppointmentSlotResponse>> GetAvailableSlotsAsync(Guid branchServiceId, DateOnly date, CancellationToken cancellationToken)
        {
            var slots = await _slotRepository.GetAvailableSlotsAsync(branchServiceId, date, cancellationToken);
            return slots.Select(s => new AppointmentSlotResponse
            {
                AppointmentSlotId = s.AppointmentSlotId,
                StartDateTimeUtc = s.StartDateTimeUtc,
                EndDateTimeUtc = s.EndDateTimeUtc
            }).ToList();
        }

        public async Task<AppointmentSlotResponse?> GetSlotByIdAsync(Guid slotId, CancellationToken cancellationToken)
        {
            var s = await _slotRepository.GetByIdAsync(slotId, cancellationToken);
            if (s == null) return null;
            return new AppointmentSlotResponse
            {
                AppointmentSlotId = s.AppointmentSlotId,
                StartDateTimeUtc = s.StartDateTimeUtc,
                EndDateTimeUtc = s.EndDateTimeUtc
            };
        }
    }
}
