using BookingSystem_Explore.Domain.Entities;
using BookingSystem_Explore.Domain.Interfaces.Repositories;

namespace BookingSystem_Explore.BusinessLibrary.Repositories
{
    public class InMemoryBranchRepository : IBranchRepository
    {
        private readonly InMemoryDataStore _store;

        public InMemoryBranchRepository(InMemoryDataStore store)
        {
            _store = store;
        }

        public Task<IReadOnlyList<Branch>> GetActiveBranchesAsync(CancellationToken cancellationToken)
        {
            var list = _store.Branches.Where(b => b.IsActive).ToList();
            return Task.FromResult((IReadOnlyList<Branch>)list);
        }

        public Task<Branch?> GetByIdAsync(Guid branchId, CancellationToken cancellationToken)
        {
            var b = _store.Branches.FirstOrDefault(x => x.BranchId == branchId);
            return Task.FromResult(b);
        }

        public Task<IReadOnlyList<BranchService>> GetBranchServicesAsync(Guid branchId, CancellationToken cancellationToken)
        {
            var services = _store.BranchServices.Where(bs => bs.BranchId == branchId).ToList();
            // attach Service reference
            foreach (var bs in services)
            {
                bs.Service = _store.Services.FirstOrDefault(s => s.ServiceId == bs.ServiceId);
                bs.Branch = _store.Branches.FirstOrDefault(b => b.BranchId == bs.BranchId);
            }
            return Task.FromResult((IReadOnlyList<BranchService>)services);
        }

        public Task<BranchService?> GetBranchServiceByIdAsync(Guid branchServiceId, CancellationToken cancellationToken)
        {
            var branchService = _store.BranchServices.FirstOrDefault(bs => bs.BranchServiceId == branchServiceId);
            if (branchService != null)
            {
                branchService.Service = _store.Services.FirstOrDefault(s => s.ServiceId == branchService.ServiceId);
                branchService.Branch = _store.Branches.FirstOrDefault(b => b.BranchId == branchService.BranchId);
            }

            return Task.FromResult(branchService);
        }

        public Task AddBranchServiceAsync(BranchService branchService, CancellationToken cancellationToken)
        {
            _store.BranchServices.Add(branchService);
            return Task.CompletedTask;
        }

        public Task UpdateBranchServiceAsync(BranchService branchService, CancellationToken cancellationToken)
        {
            var existing = _store.BranchServices.FirstOrDefault(bs => bs.BranchServiceId == branchService.BranchServiceId);
            if (existing != null)
            {
                existing.Price = branchService.Price;
                existing.IsActive = branchService.IsActive;
                existing.UpdatedDate = branchService.UpdatedDate;
                existing.ServiceId = branchService.ServiceId;
                existing.BranchId = branchService.BranchId;
            }

            return Task.CompletedTask;
        }
    }
}
