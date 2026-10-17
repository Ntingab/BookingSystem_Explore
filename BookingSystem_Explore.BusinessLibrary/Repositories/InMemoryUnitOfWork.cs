using BookingSystem_Explore.Domain.Interfaces;

namespace BookingSystem_Explore.BusinessLibrary.Repositories
{
    public class InMemoryUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            // In-memory store changes are immediate
            return Task.FromResult(0);
        }
    }
}
