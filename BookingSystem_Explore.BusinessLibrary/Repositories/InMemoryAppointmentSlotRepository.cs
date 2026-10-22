using BookingSystem_Explore.Domain.Entities;
using BookingSystem_Explore.Domain.Interfaces.Repositories;

namespace BookingSystem_Explore.BusinessLibrary.Repositories
{
    public class InMemoryAppointmentSlotRepository : IAppointmentSlotRepository
    {
        private readonly InMemoryDataStore _store;

        public InMemoryAppointmentSlotRepository(InMemoryDataStore store)
        {
            _store = store;
        }

        public Task<AppointmentSlot?> GetByIdAsync(Guid slotId, CancellationToken cancellationToken)
        {
            var s = _store.Slots.FirstOrDefault(x => x.AppointmentSlotId == slotId);
            if (s != null)
            {
                s.BranchService = _store.BranchServices.FirstOrDefault(bs => bs.BranchServiceId == s.BranchServiceId);
                if (s.BranchService != null)
                {
                    s.BranchService.Service = _store.Services.FirstOrDefault(se => se.ServiceId == s.BranchService.ServiceId);
                    s.BranchService.Branch = _store.Branches.FirstOrDefault(b => b.BranchId == s.BranchService.BranchId);
                }
            }
            return Task.FromResult(s);
        }

        public Task<IReadOnlyList<AppointmentSlot>> GetAvailableSlotsAsync(Guid branchServiceId, DateOnly date, CancellationToken cancellationToken)
        {
            var occupiedSlotIds = GetOccupiedSlotIds();
            var slots = _store.Slots
                .Where(s => s.BranchServiceId == branchServiceId
                            && s.IsActive
                            && s.StartDateTimeUtc.Date == date.ToDateTime(new TimeOnly()).Date
                            && !occupiedSlotIds.Contains(s.AppointmentSlotId))
                .ToList();
            return Task.FromResult((IReadOnlyList<AppointmentSlot>)slots);
        }

        public Task<IReadOnlyList<AppointmentSlot>> GetAllAvailableSlotsAsync(CancellationToken cancellationToken)
        {
            var occupiedSlotIds = GetOccupiedSlotIds();
            var slots = _store.Slots
                .Where(s => s.IsActive && !occupiedSlotIds.Contains(s.AppointmentSlotId))
                .ToList();
            return Task.FromResult((IReadOnlyList<AppointmentSlot>)slots);
        }

        public Task AddAsync(AppointmentSlot slot, CancellationToken cancellationToken)
        {
            _store.Slots.Add(slot);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(AppointmentSlot slot, CancellationToken cancellationToken)
        {
            var idx = _store.Slots.FindIndex(s => s.AppointmentSlotId == slot.AppointmentSlotId);
            if (idx >= 0)
            {
                _store.Slots[idx] = slot;
            }
            return Task.CompletedTask;
        }

        private HashSet<Guid> GetOccupiedSlotIds()
        {
            var occupiedStatuses = new[]
            {
                BookingSystem_Explore.Domain.Enums.AppointmentStatus.Confirmed,
                BookingSystem_Explore.Domain.Enums.AppointmentStatus.Completed,
                BookingSystem_Explore.Domain.Enums.AppointmentStatus.NoShow
            };

            return _store.Appointments
                .Where(a => occupiedStatuses.Contains(a.Status))
                .Select(a => a.AppointmentSlotId)
                .ToHashSet();
        }
    }
}
