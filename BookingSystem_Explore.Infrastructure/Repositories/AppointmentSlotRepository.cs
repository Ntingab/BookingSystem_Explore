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
            // Load branch service and branch timezone
            var bs = await _db.BranchServices.Include(b => b.Branch).FirstOrDefaultAsync(b => b.BranchServiceId == branchServiceId, cancellationToken);
            if (bs == null) return Array.Empty<AppointmentSlot>();

            var tz = TimeZoneInfo.FindSystemTimeZoneById(bs.Branch.TimeZoneId);
            var localStart = date.ToDateTime(new TimeOnly(0,0));
            var localEnd = localStart.AddDays(1);
            var utcStart = TimeZoneInfo.ConvertTimeToUtc(localStart, tz);
            var utcEnd = TimeZoneInfo.ConvertTimeToUtc(localEnd, tz);

            var occupiedStatuses = new[] { BookingSystem_Explore.Domain.Enums.AppointmentStatus.Confirmed, BookingSystem_Explore.Domain.Enums.AppointmentStatus.Cancelled };

            var slots = await _db.AppointmentSlots
                .Where(s => s.BranchServiceId == branchServiceId && s.IsActive && s.StartDateTimeUtc >= utcStart && s.StartDateTimeUtc < utcEnd)
                .Where(s => !_db.Appointments.Any(a => a.AppointmentSlotId == s.AppointmentSlotId && occupiedStatuses.Contains(a.Status)))
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return slots;
        }
    }
}
