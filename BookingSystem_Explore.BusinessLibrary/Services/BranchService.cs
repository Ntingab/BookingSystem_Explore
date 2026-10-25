using BookingSystem_Explore.BusinessLibrary.DTOs.Requests;
using BookingSystem_Explore.BusinessLibrary.DTOs.Responses;
using BookingSystem_Explore.BusinessLibrary.Interfaces.Services;
using BookingSystem_Explore.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace BookingSystem_Explore.BusinessLibrary.Services
{
    public class BranchService : IBranchService
    {
        private readonly IBranchRepository _branchRepository;
        private readonly BookingSystem_Explore.Domain.Interfaces.IUnitOfWork _unitOfWork;
        private readonly ILogger<BranchService> _logger;

        public BranchService(
            IBranchRepository branchRepository,
            BookingSystem_Explore.Domain.Interfaces.IUnitOfWork unitOfWork,
            ILogger<BranchService> logger)
        {
            _branchRepository = branchRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IReadOnlyList<BranchResponse>> GetBranchesAsync(CancellationToken cancellationToken, bool includeInactive = false)
        {
            var branches = await _branchRepository.GetBranchesAsync(includeInactive, cancellationToken);
            return branches.Select(ToBranchResponse).ToList();
        }

        public async Task<BranchResponse?> GetBranchByIdAsync(Guid branchId, CancellationToken cancellationToken)
        {
            var b = await _branchRepository.GetByIdAsync(branchId, cancellationToken);
            if (b == null) return null;
            return ToBranchResponse(b);
        }

        public async Task<BranchResponse> AddBranchAsync(CreateBranchRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var branch = new BookingSystem_Explore.Domain.Entities.Branch
                {
                    BranchId = Guid.NewGuid(),
                    CreatedDate = DateTime.UtcNow
                };

                Apply(branch, request);
                await _branchRepository.AddAsync(branch, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return ToBranchResponse(branch);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add branch {BranchCode}", request.BranchCode);
                throw;
            }
            
        }

        public async Task<BranchResponse?> UpdateBranchAsync(Guid branchId, UpdateBranchRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var branch = await _branchRepository.GetByIdAsync(branchId, cancellationToken);
                if (branch is null)
                {
                    return null;
                }

                Apply(branch, request);
                branch.UpdatedDate = DateTime.UtcNow;
                await _branchRepository.UpdateAsync(branch, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return ToBranchResponse(branch);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update branch {BranchId}", branchId);
                throw;
            }
           
        }

        public async Task<IReadOnlyList<BranchServiceResponse>> GetBranchServicesAsync(Guid branchId, CancellationToken cancellationToken)
        {
            var services = await _branchRepository.GetBranchServicesAsync(branchId, cancellationToken);
            return services.Select(ToBranchServiceResponse).ToList();
        }

        public async Task<BranchServiceResponse> AddBranchServiceAsync(Guid branchId, CreateBranchServiceRequest request, CancellationToken cancellationToken)
        {

            try
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
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to add service {ServiceId} to branch {BranchId}",
                    request.ServiceId,
                    branchId);
                throw;
            }
            
        }

        public async Task<BranchServiceResponse?> UpdateBranchServiceAsync(Guid branchId, Guid branchServiceId, UpdateBranchServiceRequest request, CancellationToken cancellationToken)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to update branch service {BranchServiceId} for branch {BranchId}",
                    branchServiceId,
                    branchId);
                throw;
            }
           
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

        private static void Apply(BookingSystem_Explore.Domain.Entities.Branch branch, CreateBranchRequest request)
        {
            branch.BranchCode = request.BranchCode.Trim();
            branch.BranchName = request.BranchName.Trim();
            branch.Address = request.Address.Trim();
            branch.City = request.City.Trim();
            branch.Province = request.Province.Trim();
            branch.ContactNumber = request.ContactNumber.Trim();
            branch.TimeZoneId = request.TimeZoneId.Trim();
            branch.IsActive = request.IsActive;
        }

        private static BranchResponse ToBranchResponse(BookingSystem_Explore.Domain.Entities.Branch branch)
        {
            return new BranchResponse
            {
                BranchId = branch.BranchId,
                BranchCode = branch.BranchCode,
                BranchName = branch.BranchName,
                Address = branch.Address,
                City = branch.City,
                Province = branch.Province,
                ContactNumber = branch.ContactNumber,
                TimeZoneId = branch.TimeZoneId,
                IsActive = branch.IsActive
            };
        }
    }
}
