using BookingSystem_Explore.Domain.Entities;

namespace BookingSystem_Explore.BusinessLibrary.Repositories
{
    public class InMemoryDataStore
    {
        public List<Branch> Branches { get; } = new();
        public List<Service> Services { get; } = new();
        public List<BranchService> BranchServices { get; } = new();
        public List<AppointmentSlot> Slots { get; } = new();
        public List<Customer> Customers { get; } = new();
        public List<Appointment> Appointments { get; } = new();
        public List<Notification> Notifications { get; } = new();

        public InMemoryDataStore()
        {
            // Seed deterministic data (three branches, four services)
            var sandton = new Branch { BranchId = Guid.Parse("11111111-1111-1111-1111-111111111111"), BranchCode = "SAND", BranchName = "Sandton Branch", City = "Sandton", Province = "Gauteng", TimeZoneId = "Africa/Johannesburg", Address = "Sandton Address", CreatedDate = DateTime.UtcNow };
            var pretoria = new Branch { BranchId = Guid.Parse("22222222-2222-2222-2222-222222222222"), BranchCode = "PRET", BranchName = "Pretoria Branch", City = "Pretoria", Province = "Gauteng", TimeZoneId = "Africa/Johannesburg", Address = "Pretoria Address", CreatedDate = DateTime.UtcNow };
            var capetown = new Branch { BranchId = Guid.Parse("33333333-3333-3333-3333-333333333333"), BranchCode = "CT", BranchName = "Cape Town Branch", City = "Cape Town", Province = "Western Cape", TimeZoneId = "Africa/Johannesburg", Address = "Cape Town Address", CreatedDate = DateTime.UtcNow };
            Branches.AddRange(new[] { sandton, pretoria, capetown });

            var svc1 = new Service { ServiceId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), ServiceName = "Account Opening", DurationMinutes = 30, CreatedDate = DateTime.UtcNow };
            var svc2 = new Service { ServiceId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), ServiceName = "Card Collection", DurationMinutes = 15, CreatedDate = DateTime.UtcNow };
            var svc3 = new Service { ServiceId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), ServiceName = "Loan Consultation", DurationMinutes = 45, CreatedDate = DateTime.UtcNow };
            var svc4 = new Service { ServiceId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"), ServiceName = "General Enquiry", DurationMinutes = 30, CreatedDate = DateTime.UtcNow };
            Services.AddRange(new[] { svc1, svc2, svc3, svc4 });

            // branch-service associations
            BranchServices.Add(new BranchService { BranchServiceId = Guid.NewGuid(), BranchId = sandton.BranchId, ServiceId = svc1.ServiceId, CreatedDate = DateTime.UtcNow });
            BranchServices.Add(new BranchService { BranchServiceId = Guid.NewGuid(), BranchId = sandton.BranchId, ServiceId = svc2.ServiceId, CreatedDate = DateTime.UtcNow });
            BranchServices.Add(new BranchService { BranchServiceId = Guid.NewGuid(), BranchId = pretoria.BranchId, ServiceId = svc1.ServiceId, CreatedDate = DateTime.UtcNow });
            BranchServices.Add(new BranchService { BranchServiceId = Guid.NewGuid(), BranchId = capetown.BranchId, ServiceId = svc4.ServiceId, CreatedDate = DateTime.UtcNow });

            // simple slots for next 3 days for first branch-service
            var bs = BranchServices[0];
            var local = DateTime.UtcNow.Date.AddHours(9);
            for (int d = 0; d < 3; d++)
            {
                var start = local.AddDays(d);
                var end = start.AddMinutes(svc1.DurationMinutes);
                Slots.Add(new AppointmentSlot { AppointmentSlotId = Guid.NewGuid(), BranchServiceId = bs.BranchServiceId, StartDateTimeUtc = start, EndDateTimeUtc = end, CreatedDate = DateTime.UtcNow });
            }
        }
    }
}
