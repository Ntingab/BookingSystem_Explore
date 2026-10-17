using BookingSystem_Explore.Domain.Entities;
using BookingSystem_Explore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem_Explore.Infrastructure.Repositories
{
    public class BranchRepository : BookingSystem_Explore.Domain.Interfaces.Repositories.IBranchRepository
    {
        private readonly AppointmentBookingDbContext _db;

        public BranchRepository(AppointmentBookingDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<Branch>> GetActiveBranchesAsync(CancellationToken cancellationToken)
        {
            return await _db.Branches.AsNoTracking().Where(b => b.IsActive).ToListAsync(cancellationToken);
        }

        public async Task<Branch?> GetByIdAsync(Guid branchId, CancellationToken cancellationToken)
        {
            return await _db.Branches.AsNoTracking().FirstOrDefaultAsync(b => b.BranchId == branchId, cancellationToken);
        }


        public async Task<IReadOnlyList<BranchService>> GetBranchServicesAsync(Guid branchId, CancellationToken cancellationToken)
        {
            return await _db.BranchServices
                .Include(bs => bs.Service)
                .Where(bs => bs.BranchId == branchId)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
    }
}
