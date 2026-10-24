using BookingSystem_Explore.Domain.Entities;
using BookingSystem_Explore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem_Explore.Infrastructure.Repositories
{
    public class AppointmentSlotRepository : BookingSystem_Explore.Domain.Interfaces.Repositories.IAppointmentSlotRepository
    {
        private readonly AppointmentBookingDbContext _db;

        public AppointmentSlotRepository(AppointmentBookingDbContext db)
        {
            _db = db;
        }

        public async Task<AppointmentSlot?> GetByIdAsync(Guid slotId, CancellationToken cancellationToken)
        {
            return await _db.AppointmentSlots
                .Include(s => s.BranchService)
                    .ThenInclude(bs => bs.Service)
                .Include(s => s.BranchService)
                    .ThenInclude(bs => bs.Branch)
                .FirstOrDefaultAsync(s => s.AppointmentSlotId == slotId, cancellationToken);
        }

        public async Task<IReadOnlyList<AppointmentSlot>> GetAvailableSlotsAsync(Guid branchServiceId, DateOnly date, CancellationToken cancellationToken)
        {
            var timeZoneId = await _db.BranchServices
                .Where(branchService => branchService.BranchServiceId == branchServiceId)
                .Select(branchService => branchService.Branch.TimeZoneId)
                .SingleOrDefaultAsync(cancellationToken);
            if (timeZoneId is null) return [];

            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            var localStart = date.ToDateTime(new TimeOnly(0,0));
            var localEnd = localStart.AddDays(1);
            var utcStart = TimeZoneInfo.ConvertTimeToUtc(localStart, tz);
            var utcEnd = TimeZoneInfo.ConvertTimeToUtc(localEnd, tz);

            var slots = await _db.AppointmentSlots
                .Where(s => s.BranchServiceId == branchServiceId && s.IsActive && s.StartDateTimeUtc >= utcStart && s.StartDateTimeUtc < utcEnd)
                .Where(s => !_db.Appointments.Any(a => a.AppointmentSlotId == s.AppointmentSlotId &&
                    (a.Status == BookingSystem_Explore.Domain.Enums.AppointmentStatus.Confirmed ||
                     a.Status == BookingSystem_Explore.Domain.Enums.AppointmentStatus.Completed ||
                     a.Status == BookingSystem_Explore.Domain.Enums.AppointmentStatus.NoShow)))
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return slots;
        }

        public async Task<IReadOnlyList<AppointmentSlot>> GetAllAvailableSlotsAsync(CancellationToken cancellationToken)
        {
            return await _db.AppointmentSlots
                .Where(s => s.IsActive)
                .Where(s => !_db.Appointments.Any(a => a.AppointmentSlotId == s.AppointmentSlotId &&
                    (a.Status == BookingSystem_Explore.Domain.Enums.AppointmentStatus.Confirmed ||
                     a.Status == BookingSystem_Explore.Domain.Enums.AppointmentStatus.Completed ||
                     a.Status == BookingSystem_Explore.Domain.Enums.AppointmentStatus.NoShow)))
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(AppointmentSlot slot, CancellationToken cancellationToken)
        {
            await _db.AppointmentSlots.AddAsync(slot, cancellationToken);
        }

        public Task UpdateAsync(AppointmentSlot slot, CancellationToken cancellationToken)
        {
            _db.AppointmentSlots.Update(slot);
            return Task.CompletedTask;
        }

    }
}
