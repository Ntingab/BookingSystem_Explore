using BookingSystem_Explore.BusinessLibrary.DTOs.Responses;

namespace BookingSystem_Explore.BusinessLibrary.Interfaces.Services
{
    public interface IServiceService
    {
        Task<IReadOnlyList<ServiceResponse>> GetServicesAsync(CancellationToken cancellationToken);
        Task<ServiceResponse?> GetServiceByIdAsync(Guid serviceId, CancellationToken cancellationToken);
        Task<ServiceResponse> AddServiceAsync(BookingSystem_Explore.BusinessLibrary.DTOs.Requests.CreateServiceRequest request, CancellationToken cancellationToken);
        Task<ServiceResponse?> UpdateServiceAsync(Guid serviceId, BookingSystem_Explore.BusinessLibrary.DTOs.Requests.UpdateServiceRequest request, CancellationToken cancellationToken);
    }
}
