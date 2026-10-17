using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BookingSystem_Explore.Domain.Interfaces.Repositories;
using BookingSystem_Explore.Domain.Entities;

namespace BookingSystem_Explore.BusinessLibrary.Repositories
{
    public class InMemoryCustomerRepository : ICustomerRepository
    {
        private readonly InMemoryDataStore _store;

        public InMemoryCustomerRepository(InMemoryDataStore store)
        {
            _store = store;
        }

        public Task AddAsync(Customer customer, CancellationToken cancellationToken)
        {
            _store.Customers.Add(customer);
            return Task.CompletedTask;
        }

        public Task<Customer?> GetByIdAsync(Guid customerId, CancellationToken cancellationToken)
        {
            var c = _store.Customers.FirstOrDefault(x => x.CustomerId == customerId);
            return Task.FromResult(c);
        }
    }
}
