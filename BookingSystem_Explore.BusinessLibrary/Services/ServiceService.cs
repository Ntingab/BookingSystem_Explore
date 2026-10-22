using BookingSystem_Explore.BusinessLibrary.DTOs.Responses;
using BookingSystem_Explore.BusinessLibrary.DTOs.Requests;
using BookingSystem_Explore.BusinessLibrary.Interfaces.Services;
using BookingSystem_Explore.Domain.Interfaces.Repositories;

namespace BookingSystem_Explore.BusinessLibrary.Services
{
    public class ServiceService : IServiceService
    {
        private readonly IServiceRepository _serviceRepository;
        private readonly BookingSystem_Explore.Domain.Interfaces.IUnitOfWork _unitOfWork;

        public ServiceService(IServiceRepository serviceRepository, BookingSystem_Explore.Domain.Interfaces.IUnitOfWork unitOfWork)
        {
            _serviceRepository = serviceRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<IReadOnlyList<ServiceResponse>> GetServicesAsync(CancellationToken cancellationToken)
        {
            var svcs = await _serviceRepository.GetActiveServicesAsync(cancellationToken);
            return svcs.Select(ToResponse).ToList();
        }

        public async Task<ServiceResponse?> GetServiceByIdAsync(Guid serviceId, CancellationToken cancellationToken)
        {
            var s = await _serviceRepository.GetByIdAsync(serviceId, cancellationToken);
            if (s == null) return null;
            return ToResponse(s);
        }

        public async Task<ServiceResponse> AddServiceAsync(CreateServiceRequest request, CancellationToken cancellationToken)
        {
            var service = new BookingSystem_Explore.Domain.Entities.Service
            {
                ServiceId = Guid.NewGuid(),
                ServiceName = request.ServiceName,
                DurationMinutes = request.DurationMinutes,
                Description = request.Description,
                IsActive = request.IsActive,
                CreatedDate = DateTime.UtcNow
            };

            await _serviceRepository.AddAsync(service, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ToResponse(service);
        }

        public async Task<ServiceResponse?> UpdateServiceAsync(Guid serviceId, UpdateServiceRequest request, CancellationToken cancellationToken)
        {
            var service = await _serviceRepository.GetByIdAsync(serviceId, cancellationToken);
            if (service == null)
            {
                return null;
            }

            service.ServiceName = request.ServiceName;
            service.DurationMinutes = request.DurationMinutes;
            service.Description = request.Description;
            service.IsActive = request.IsActive;
            service.UpdatedDate = DateTime.UtcNow;

            await _serviceRepository.UpdateAsync(service, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ToResponse(service);
        }

        private static ServiceResponse ToResponse(BookingSystem_Explore.Domain.Entities.Service service)
        {
            return new ServiceResponse
            {
                ServiceId = service.ServiceId,
                ServiceName = service.ServiceName,
                DurationMinutes = service.DurationMinutes,
                Description = service.Description,
                IsActive = service.IsActive
            };
        }
    }
}
