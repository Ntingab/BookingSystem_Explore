using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using BookingSystem_Explore.Infrastructure.Persistence;

#nullable disable

namespace BookingSystem_Explore.Infrastructure.Migrations
{
    [DbContext(typeof(AppointmentBookingDbContext))]
    partial class AppointmentBookingDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            // Minimal snapshot to satisfy EF tooling in-source. The full model is built at runtime
            // by applying configurations in AppointmentBookingDbContext.OnModelCreating.
            modelBuilder
                .HasAnnotation("ProductVersion", "EFCore")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);
        }
    }
}
