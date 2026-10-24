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

        public async Task<IReadOnlyList<Branch>> GetBranchesAsync(bool includeInactive, CancellationToken cancellationToken)
        {
            var query = _db.Branches.AsNoTracking();
            return await (includeInactive ? query : query.Where(b => b.IsActive)).ToListAsync(cancellationToken);
        }

        public async Task<Branch?> GetByIdAsync(Guid branchId, CancellationToken cancellationToken)
        {
            return await _db.Branches.AsNoTracking().FirstOrDefaultAsync(b => b.BranchId == branchId, cancellationToken);
        }

        public async Task AddAsync(Branch branch, CancellationToken cancellationToken)
        {
            await _db.Branches.AddAsync(branch, cancellationToken);
        }

        public Task UpdateAsync(Branch branch, CancellationToken cancellationToken)
        {
            _db.Branches.Update(branch);
            return Task.CompletedTask;
        }

        public async Task<IReadOnlyList<BranchService>> GetBranchServicesAsync(Guid branchId, CancellationToken cancellationToken)
        {
            return await _db.BranchServices
                .Include(bs => bs.Service)
                .Where(bs => bs.BranchId == branchId)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<BranchService?> GetBranchServiceByIdAsync(Guid branchServiceId, CancellationToken cancellationToken)
        {
            return await _db.BranchServices
                .Include(bs => bs.Service)
                .AsNoTracking()
                .FirstOrDefaultAsync(bs => bs.BranchServiceId == branchServiceId, cancellationToken);
        }

        public async Task AddBranchServiceAsync(BranchService branchService, CancellationToken cancellationToken)
        {
            await _db.BranchServices.AddAsync(branchService, cancellationToken);
        }

        public async Task UpdateBranchServiceAsync(BranchService branchService, CancellationToken cancellationToken)
        {
            _db.BranchServices.Update(branchService);
            await Task.CompletedTask;
        }
    }
}
