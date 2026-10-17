using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using BookingSystem_Explore.Domain.Interfaces.Repositories;
using BookingSystem_Explore.Domain.Entities;

namespace BookingSystem_Explore.BusinessLibrary.Repositories
{
    public class InMemoryAppointmentRepository : IAppointmentRepository
    {
        private readonly InMemoryDataStore _store;

        public InMemoryAppointmentRepository(InMemoryDataStore store)
        {
            _store = store;
        }

        public Task AddAsync(Appointment appointment, CancellationToken cancellationToken)
        {
            _store.Appointments.Add(appointment);
            return Task.CompletedTask;
        }

        public Task<Appointment?> GetByIdAsync(Guid appointmentId, CancellationToken cancellationToken)
        {
            var a = _store.Appointments.FirstOrDefault(x => x.AppointmentId == appointmentId);
            if (a != null)
            {
                a.AppointmentSlot = _store.Slots.FirstOrDefault(s => s.AppointmentSlotId == a.AppointmentSlotId);
                a.Customer = _store.Customers.FirstOrDefault(c => c.CustomerId == a.CustomerId);
                a.Notifications = _store.Notifications.Where(n => n.AppointmentId == a.AppointmentId).ToList();
                if (a.AppointmentSlot != null)
                {
                    a.AppointmentSlot.BranchService = _store.BranchServices.FirstOrDefault(bs => bs.BranchServiceId == a.AppointmentSlot.BranchServiceId);
                    if (a.AppointmentSlot.BranchService != null)
                    {
                        a.AppointmentSlot.BranchService.Branch = _store.Branches.FirstOrDefault(b => b.BranchId == a.AppointmentSlot.BranchService.BranchId);
                        a.AppointmentSlot.BranchService.Service = _store.Services.FirstOrDefault(s => s.ServiceId == a.AppointmentSlot.BranchService.ServiceId);
                    }
                }
            }
            return Task.FromResult(a);
        }

        public Task<Appointment?> GetByReferenceAsync(string reference, CancellationToken cancellationToken)
        {
            var a = _store.Appointments.FirstOrDefault(x => x.AppointmentReference == reference);
            return GetByIdAsync(a?.AppointmentId ?? Guid.Empty, cancellationToken);
        }

        public Task<bool> IsSlotOccupiedAsync(Guid appointmentSlotId, CancellationToken cancellationToken)
        {
            var occupiedStatuses = new[] { BookingSystem_Explore.Domain.Enums.AppointmentStatus.Confirmed, BookingSystem_Explore.Domain.Enums.AppointmentStatus.Completed, BookingSystem_Explore.Domain.Enums.AppointmentStatus.NoShow };
            var occupied = _store.Appointments.Any(a => a.AppointmentSlotId == appointmentSlotId && occupiedStatuses.Contains(a.Status));
            return Task.FromResult(occupied);
        }
    }
}
