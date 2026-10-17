using System;
using System.Threading;
using System.Threading.Tasks;
using BookingSystem_Explore.BusinessLibrary.DTOs.Requests;
using BookingSystem_Explore.BusinessLibrary.DTOs.Responses;

namespace BookingSystem_Explore.BusinessLibrary.Interfaces.Services
{
    public interface IAppointmentService
    {
        Task<AppointmentResponse> BookAppointmentAsync(CreateAppointmentRequest request, CancellationToken cancellationToken);
        Task<AppointmentResponse?> GetAppointmentAsync(Guid appointmentId, string managementToken, CancellationToken cancellationToken);
        Task<AppointmentResponse?> GetAppointmentByReferenceAsync(string reference, string managementToken, CancellationToken cancellationToken);
        Task<AppointmentResponse> CancelAppointmentAsync(Guid appointmentId, string managementToken, CancellationToken cancellationToken);
    }
}
