using System;
using System.Threading;
using System.Threading.Tasks;
using BookingSystem_Explore.Infrastructure.Entities;

namespace BookingSystem_Explore.BusinessLibrary.Interfaces.Repositories
{
    public interface ICustomerRepository
    {
        Task<Customer?> GetByIdAsync(Guid customerId, CancellationToken cancellationToken);
        Task AddAsync(Customer customer, CancellationToken cancellationToken);
    }
}
