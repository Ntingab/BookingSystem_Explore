using BookingSystem_Explore.BusinessLibrary.DTOs.Responses;
using BookingSystem_Explore.BusinessLibrary.DTOs.Requests;
using BookingSystem_Explore.BusinessLibrary.Interfaces.Services;
using BookingSystem_Explore.Domain.Interfaces.Repositories;

namespace BookingSystem_Explore.BusinessLibrary.Services
{
    public class BranchService : IBranchService
    {
        private readonly IBranchRepository _branchRepository;
        private readonly BookingSystem_Explore.Domain.Interfaces.IUnitOfWork _unitOfWork;

        public BranchService(IBranchRepository branchRepository, BookingSystem_Explore.Domain.Interfaces.IUnitOfWork unitOfWork)
        {
            _branchRepository = branchRepository;
            _unitOfWork = unitOfWork;
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

        public async Task<IReadOnlyList<BranchServiceResponse>> GetBranchServicesAsync(Guid branchId, CancellationToken cancellationToken)
        {
            var services = await _branchRepository.GetBranchServicesAsync(branchId, cancellationToken);
            return services.Select(ToBranchServiceResponse).ToList();
        }

        public async Task<BranchServiceResponse> AddBranchServiceAsync(Guid branchId, CreateBranchServiceRequest request, CancellationToken cancellationToken)
        {
            var branchService = new BookingSystem_Explore.Domain.Entities.BranchService
            {
                BranchServiceId = Guid.NewGuid(),
                BranchId = branchId,
                ServiceId = request.ServiceId,
                Price = request.Price,
                IsActive = request.IsActive,
                CreatedDate = DateTime.UtcNow
            };

            await _branchRepository.AddBranchServiceAsync(branchService, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var created = await _branchRepository.GetBranchServiceByIdAsync(branchService.BranchServiceId, cancellationToken);
            return ToBranchServiceResponse(created ?? branchService);
        }

        public async Task<BranchServiceResponse?> UpdateBranchServiceAsync(Guid branchId, Guid branchServiceId, UpdateBranchServiceRequest request, CancellationToken cancellationToken)
        {
            var existing = await _branchRepository.GetBranchServiceByIdAsync(branchServiceId, cancellationToken);
            if (existing == null || existing.BranchId != branchId)
            {
                return null;
            }

            existing.Price = request.Price;
            existing.IsActive = request.IsActive;
            existing.UpdatedDate = DateTime.UtcNow;

            await _branchRepository.UpdateBranchServiceAsync(existing, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var updated = await _branchRepository.GetBranchServiceByIdAsync(branchServiceId, cancellationToken);
            return ToBranchServiceResponse(updated ?? existing);
        }

        private static BranchServiceResponse ToBranchServiceResponse(BookingSystem_Explore.Domain.Entities.BranchService branchService)
        {
            return new BranchServiceResponse
            {
                BranchServiceId = branchService.BranchServiceId,
                BranchId = branchService.BranchId,
                ServiceId = branchService.ServiceId,
                ServiceName = branchService.Service?.ServiceName ?? string.Empty,
                DurationMinutes = branchService.Service?.DurationMinutes ?? 0,
                Price = branchService.Price,
                IsActive = branchService.IsActive
            };
        }
    }
}
