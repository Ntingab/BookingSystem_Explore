using BookingSystem_Explore.Domain.Entities;
using BookingSystem_Explore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem_Explore.Infrastructure.Repositories
{
    public class AppointmentRepository : BookingSystem_Explore.Domain.Interfaces.Repositories.IAppointmentRepository
    {
        private readonly AppointmentBookingDbContext _db;

        public AppointmentRepository(AppointmentBookingDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(Appointment appointment, CancellationToken cancellationToken)
        {
            await _db.Appointments.AddAsync(appointment, cancellationToken);
        }

        public async Task<Appointment?> GetByIdAsync(Guid appointmentId, CancellationToken cancellationToken)
        {
            return await _db.Appointments
                .Include(a => a.AppointmentSlot)
                    .ThenInclude(s => s.BranchService)
                        .ThenInclude(bs => bs.Branch)
                .Include(a => a.AppointmentSlot)
                    .ThenInclude(s => s.BranchService)
                        .ThenInclude(bs => bs.Service)
                .Include(a => a.Customer)
                .Include(a => a.Notifications)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId, cancellationToken);
        }

        public async Task<Appointment?> GetByReferenceAsync(string reference, CancellationToken cancellationToken)
        {
            return await _db.Appointments
                .Include(a => a.AppointmentSlot)
                    .ThenInclude(s => s.BranchService)
                        .ThenInclude(bs => bs.Branch)
                .Include(a => a.AppointmentSlot)
                    .ThenInclude(s => s.BranchService)
                        .ThenInclude(bs => bs.Service)
                .Include(a => a.Customer)
                .Include(a => a.Notifications)
                .FirstOrDefaultAsync(a => a.AppointmentReference == reference, cancellationToken);
        }

        public async Task<bool> IsSlotOccupiedAsync(Guid appointmentSlotId, CancellationToken cancellationToken)
        {
            var occupiedStatuses = new List<BookingSystem_Explore.Domain.Enums.AppointmentStatus>
            {
                BookingSystem_Explore.Domain.Enums.AppointmentStatus.Confirmed,
                BookingSystem_Explore.Domain.Enums.AppointmentStatus.Completed,
                BookingSystem_Explore.Domain.Enums.AppointmentStatus.NoShow
            };

            return await _db.Appointments.AnyAsync(a => a.AppointmentSlotId == appointmentSlotId && occupiedStatuses.Contains(a.Status), cancellationToken);
        }
    }
}
