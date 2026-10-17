using BookingSystem_Explore.Domain.Entities;

namespace BookingSystem_Explore.Domain.Interfaces.Repositories
{
    public interface IBranchRepository
    {
        Task<IReadOnlyList<Branch>> GetActiveBranchesAsync(CancellationToken cancellationToken);
        Task<Branch?> GetByIdAsync(Guid branchId, CancellationToken cancellationToken);
        Task<IReadOnlyList<BranchService>> GetBranchServicesAsync(Guid branchId, CancellationToken cancellationToken);
    }
}
