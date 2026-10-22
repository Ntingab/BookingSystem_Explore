using BookingSystem_Explore.Domain.Entities;
using BookingSystem_Explore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem_Explore.Infrastructure.Repositories
{
    public class ServiceRepository : BookingSystem_Explore.Domain.Interfaces.Repositories.IServiceRepository
    {
        private readonly AppointmentBookingDbContext _db;

        public ServiceRepository(AppointmentBookingDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<Service>> GetActiveServicesAsync(CancellationToken cancellationToken)
        {
            return await _db.Services.AsNoTracking().Where(s => s.IsActive).ToListAsync(cancellationToken);
        }

        public async Task<Service?> GetByIdAsync(Guid serviceId, CancellationToken cancellationToken)
        {
            return await _db.Services.AsNoTracking().FirstOrDefaultAsync(s => s.ServiceId == serviceId, cancellationToken);
        }

        public async Task AddAsync(Service service, CancellationToken cancellationToken)
        {
            await _db.Services.AddAsync(service, cancellationToken);
        }

        public async Task UpdateAsync(Service service, CancellationToken cancellationToken)
        {
            _db.Services.Update(service);
            await Task.CompletedTask;
        }
    }
}
