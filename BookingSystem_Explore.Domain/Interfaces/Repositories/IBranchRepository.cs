using BookingSystem_Explore.Domain.Entities;

namespace BookingSystem_Explore.Domain.Interfaces.Repositories
{
    public interface IBranchRepository
    {
        Task<IReadOnlyList<Branch>> GetBranchesAsync(bool includeInactive, CancellationToken cancellationToken);
        Task<Branch?> GetByIdAsync(Guid branchId, CancellationToken cancellationToken);
        Task AddAsync(Branch branch, CancellationToken cancellationToken);
        Task UpdateAsync(Branch branch, CancellationToken cancellationToken);
        Task<IReadOnlyList<BranchService>> GetBranchServicesAsync(Guid branchId, CancellationToken cancellationToken);
        Task<BranchService?> GetBranchServiceByIdAsync(Guid branchServiceId, CancellationToken cancellationToken);
        Task AddBranchServiceAsync(BranchService branchService, CancellationToken cancellationToken);
        Task UpdateBranchServiceAsync(BranchService branchService, CancellationToken cancellationToken);
    }
}
