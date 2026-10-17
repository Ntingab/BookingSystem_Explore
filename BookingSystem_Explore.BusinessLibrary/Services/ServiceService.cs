using BookingSystem_Explore.BusinessLibrary.DTOs.Responses;
using BookingSystem_Explore.BusinessLibrary.Interfaces.Services;
using BookingSystem_Explore.Domain.Interfaces.Repositories;

namespace BookingSystem_Explore.BusinessLibrary.Services
{
    public class ServiceService : IServiceService
    {
        private readonly IServiceRepository _serviceRepository;

        public ServiceService(IServiceRepository serviceRepository)
        {
            _serviceRepository = serviceRepository;
        }

        public async Task<IReadOnlyList<ServiceResponse>> GetServicesAsync(CancellationToken cancellationToken)
        {
            var svcs = await _serviceRepository.GetActiveServicesAsync(cancellationToken);
            return svcs.Select(s => new ServiceResponse
            {
                ServiceId = s.ServiceId,
                ServiceName = s.ServiceName,
                DurationMinutes = s.DurationMinutes
            }).ToList();
        }

        public async Task<ServiceResponse?> GetServiceByIdAsync(Guid serviceId, CancellationToken cancellationToken)
        {
            var s = await _serviceRepository.GetByIdAsync(serviceId, cancellationToken);
            if (s == null) return null;
            return new ServiceResponse
            {
                ServiceId = s.ServiceId,
                ServiceName = s.ServiceName,
                DurationMinutes = s.DurationMinutes
            };
        }
    }
}
