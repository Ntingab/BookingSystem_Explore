using BookingSystem_Explore.BusinessLibrary.DTOs.Responses;

namespace BookingSystem_Explore.BusinessLibrary.Interfaces.Services
{
    public interface IBranchService
    {
        Task<IReadOnlyList<BranchResponse>> GetBranchesAsync(CancellationToken cancellationToken, bool includeInactive = false);
        Task<BranchResponse?> GetBranchByIdAsync(Guid branchId, CancellationToken cancellationToken);
        Task<BranchResponse> AddBranchAsync(BookingSystem_Explore.BusinessLibrary.DTOs.Requests.CreateBranchRequest request, CancellationToken cancellationToken);
        Task<BranchResponse?> UpdateBranchAsync(Guid branchId, BookingSystem_Explore.BusinessLibrary.DTOs.Requests.UpdateBranchRequest request, CancellationToken cancellationToken);
        Task<IReadOnlyList<BranchServiceResponse>> GetBranchServicesAsync(Guid branchId, CancellationToken cancellationToken);
        Task<BranchServiceResponse> AddBranchServiceAsync(Guid branchId, BookingSystem_Explore.BusinessLibrary.DTOs.Requests.CreateBranchServiceRequest request, CancellationToken cancellationToken);
        Task<BranchServiceResponse?> UpdateBranchServiceAsync(Guid branchId, Guid branchServiceId, BookingSystem_Explore.BusinessLibrary.DTOs.Requests.UpdateBranchServiceRequest request, CancellationToken cancellationToken);
    }
}
