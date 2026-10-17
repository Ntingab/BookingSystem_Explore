using BookingSystem_Explore.BusinessLibrary.DTOs.Responses;

namespace BookingSystem_Explore.BusinessLibrary.Interfaces.Services
{
    public interface IBranchService
    {
        Task<IReadOnlyList<BranchResponse>> GetBranchesAsync(CancellationToken cancellationToken);
        Task<BranchResponse?> GetBranchByIdAsync(Guid branchId, CancellationToken cancellationToken);
        Task<IReadOnlyList<ServiceResponse>> GetBranchServicesAsync(Guid branchId, CancellationToken cancellationToken);
    }
}
