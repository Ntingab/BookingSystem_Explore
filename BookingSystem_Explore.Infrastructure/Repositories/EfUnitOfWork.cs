using BookingSystem_Explore.Infrastructure.Persistence;

namespace BookingSystem_Explore.Infrastructure.Repositories
{
    public class EfUnitOfWork : BookingSystem_Explore.Domain.Interfaces.IUnitOfWork
    {
        private readonly AppointmentBookingDbContext _db;

        public EfUnitOfWork(AppointmentBookingDbContext db)
        {
            _db = db;
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return _db.SaveChangesAsync(cancellationToken);
        }
    }
}
