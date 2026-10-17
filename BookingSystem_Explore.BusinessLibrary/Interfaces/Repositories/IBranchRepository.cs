using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BookingSystem_Explore.Infrastructure.Entities;

namespace BookingSystem_Explore.BusinessLibrary.Interfaces.Repositories
{
    public interface IBranchRepository
    {
        Task<IReadOnlyList<Branch>> GetActiveBranchesAsync(CancellationToken cancellationToken);
        Task<Branch?> GetByIdAsync(Guid branchId, CancellationToken cancellationToken);
        Task<IReadOnlyList<BranchService>> GetBranchServicesAsync(Guid branchId, CancellationToken cancellationToken);
    }
}
