using System.Threading;
using System.Threading.Tasks;

namespace BookingSystem_Explore.BusinessLibrary.Interfaces
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
