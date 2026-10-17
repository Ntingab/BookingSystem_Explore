using Microsoft.EntityFrameworkCore;
using BookingSystem_Explore.Domain.Entities;

namespace BookingSystem_Explore.Infrastructure.Persistence
{
    public class AppointmentBookingDbContext : DbContext
    {
        public AppointmentBookingDbContext(DbContextOptions<AppointmentBookingDbContext> options)
            : base(options)
        {
        }

        public DbSet<Branch> Branches => Set<Branch>();
        public DbSet<Service> Services => Set<Service>();
        public DbSet<BranchService> BranchServices => Set<BranchService>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<AppointmentSlot> AppointmentSlots => Set<AppointmentSlot>();
        public DbSet<Appointment> Appointments => Set<Appointment>();
        public DbSet<Notification> Notifications => Set<Notification>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppointmentBookingDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}
