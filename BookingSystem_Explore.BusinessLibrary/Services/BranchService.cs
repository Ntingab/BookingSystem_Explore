using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BookingSystem_Explore.BusinessLibrary.DTOs.Responses;
using BookingSystem_Explore.BusinessLibrary.Interfaces.Services;
using BookingSystem_Explore.Domain.Interfaces.Repositories;

namespace BookingSystem_Explore.BusinessLibrary.Services
{
    public class BranchService : IBranchService
    {
        private readonly IBranchRepository _branchRepository;

        public BranchService(IBranchRepository branchRepository)
        {
            _branchRepository = branchRepository;
        }

        public async Task<IReadOnlyList<BranchResponse>> GetBranchesAsync(CancellationToken cancellationToken)
        {
            var branches = await _branchRepository.GetActiveBranchesAsync(cancellationToken);
            return branches.Select(b => new BranchResponse
            {
                BranchId = b.BranchId,
                BranchCode = b.BranchCode,
                BranchName = b.BranchName,
                City = b.City
            }).ToList();
        }

        public async Task<BranchResponse?> GetBranchByIdAsync(Guid branchId, CancellationToken cancellationToken)
        {
            var b = await _branchRepository.GetByIdAsync(branchId, cancellationToken);
            if (b == null) return null;
            return new BranchResponse
            {
                BranchId = b.BranchId,
                BranchCode = b.BranchCode,
                BranchName = b.BranchName,
                City = b.City
            };
        }

        public async Task<IReadOnlyList<ServiceResponse>> GetBranchServicesAsync(Guid branchId, CancellationToken cancellationToken)
        {
            var services = await _branchRepository.GetBranchServicesAsync(branchId, cancellationToken);
            return services.Select(s => new ServiceResponse
            {
                ServiceId = s.ServiceId,
                ServiceName = s.Service?.ServiceName ?? string.Empty,
                DurationMinutes = s.Service?.DurationMinutes ?? 0
            }).ToList();
        }
    }
}
