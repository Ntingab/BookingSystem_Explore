using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;

namespace BookingSystem_Explore.Infrastructure.Persistence
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppointmentBookingDbContext>
    {
        public AppointmentBookingDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<AppointmentBookingDbContext>();

            // Use environment variable or fallback connection string for design-time
            var conn = Environment.GetEnvironmentVariable("DefaultConnection")
                       ?? "Server=localhost;Database=BookingSystem;User Id=sa;Password=Your_password123;";
            builder.UseSqlServer(conn);

            return new AppointmentBookingDbContext(builder.Options);
        }
    }
}
