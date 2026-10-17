using System.Threading;
using System.Threading.Tasks;

namespace BookingSystem_Explore.Domain.Interfaces
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
