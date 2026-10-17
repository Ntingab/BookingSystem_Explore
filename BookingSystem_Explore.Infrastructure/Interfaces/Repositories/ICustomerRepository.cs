using System;
using System.Threading;
using System.Threading.Tasks;
using BookingSystem_Explore.Domain.Entities;

namespace BookingSystem_Explore.Domain.Interfaces.Repositories
{
    public interface ICustomerRepository
    {
        Task<Customer?> GetByIdAsync(Guid customerId, CancellationToken cancellationToken);
        Task AddAsync(Customer customer, CancellationToken cancellationToken);
    }
}
