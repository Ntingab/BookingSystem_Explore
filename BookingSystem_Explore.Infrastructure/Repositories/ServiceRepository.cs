using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BookingSystem_Explore.Domain.Interfaces.Repositories;
using BookingSystem_Explore.Domain.Entities;
using BookingSystem_Explore.Infrastructure.Persistence;

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
    }
}
