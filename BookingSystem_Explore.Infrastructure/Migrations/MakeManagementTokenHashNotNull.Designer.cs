using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using BookingSystem_Explore.Infrastructure.Persistence;

#nullable disable

namespace BookingSystem_Explore.Infrastructure.Migrations
{
    [DbContext(typeof(AppointmentBookingDbContext))]
    [Migration("20261017_MakeManagementTokenHashNotNull")]
    partial class MakeManagementTokenHashNotNull
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            // EF Core would normally scaffold the full model here. For source-controlled
            // migrations in this repository we keep the snapshot class separate.
        }
    }
}
