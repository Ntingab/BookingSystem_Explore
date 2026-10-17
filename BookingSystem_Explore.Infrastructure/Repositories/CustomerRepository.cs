using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BookingSystem_Explore.Domain.Interfaces.Repositories;
using BookingSystem_Explore.Domain.Entities;
using BookingSystem_Explore.Infrastructure.Persistence;

namespace BookingSystem_Explore.Infrastructure.Repositories
{
    public class CustomerRepository : BookingSystem_Explore.Domain.Interfaces.Repositories.ICustomerRepository
    {
        private readonly AppointmentBookingDbContext _db;

        public CustomerRepository(AppointmentBookingDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(Customer customer, CancellationToken cancellationToken)
        {
            await _db.Customers.AddAsync(customer, cancellationToken);
        }

        public async Task<Customer?> GetByIdAsync(Guid customerId, CancellationToken cancellationToken)
        {
            return await _db.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.CustomerId == customerId, cancellationToken);
        }
    }
}
