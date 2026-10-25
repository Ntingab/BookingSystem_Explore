using BookingSystem_Explore.BusinessLibrary.DTOs.Requests;
using BookingSystem_Explore.Domain.Entities;
using BookingSystem_Explore.Domain.Interfaces;
using BookingSystem_Explore.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using BranchEntity = BookingSystem_Explore.Domain.Entities.Branch;
using BranchServiceEntity = BookingSystem_Explore.Domain.Entities.BranchService;
using BranchService = BookingSystem_Explore.BusinessLibrary.Services.BranchService;

namespace BookingSystem_Explore.Tests.BusinessLibrary
{
    public class BranchServiceTests
    {
        private readonly Mock<IBranchRepository> _branchRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ILogger<BranchService>> _loggerMock;
        private readonly BranchService _sut;

        public BranchServiceTests()
        {
            _branchRepositoryMock = new Mock<IBranchRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _loggerMock = new Mock<ILogger<BranchService>>();

            _sut = new BranchService(
                _branchRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _loggerMock.Object);
        }

        #region GetBranchesAsync

        [Fact]
        public async Task GetBranchesAsync_WhenBranchesExist_ReturnsMappedBranches()
        {
            // Arrange
            var branches = new List<BranchEntity>
            {
                CreateBranch(),
                CreateBranch()
            };

            _branchRepositoryMock
                .Setup(x => x.GetBranchesAsync(
                    false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(branches);

            // Act
            var result = await _sut.GetBranchesAsync(
                CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, branch =>
            {
                Assert.Equal("JHB", branch.BranchCode);
                Assert.Equal("Johannesburg Branch", branch.BranchName);
                Assert.Equal("Johannesburg", branch.City);
            });

            _branchRepositoryMock.Verify(
                x => x.GetBranchesAsync(
                    false, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetBranchesAsync_WhenNoBranches_ReturnsEmptyList()
        {
            // Arrange
            _branchRepositoryMock
                .Setup(x => x.GetBranchesAsync(
                    false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<BranchEntity>());

            // Act
            var result = await _sut.GetBranchesAsync(
                CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetBranchesAsync_WhenIncludeInactiveIsTrue_PassesFlagToRepository()
        {
            // Arrange
            _branchRepositoryMock
                .Setup(x => x.GetBranchesAsync(
                    true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<BranchEntity>());

            // Act
            await _sut.GetBranchesAsync(
                CancellationToken.None,
                includeInactive: true);

            // Assert
            _branchRepositoryMock.Verify(
                x => x.GetBranchesAsync(
                    true, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        #endregion

        #region GetBranchByIdAsync

        [Fact]
        public async Task GetBranchByIdAsync_WhenBranchExists_ReturnsBranch()
        {
            // Arrange
            var branch = CreateBranch();

            _branchRepositoryMock
                .Setup(x => x.GetByIdAsync(
                    branch.BranchId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(branch);

            // Act
            var result = await _sut.GetBranchByIdAsync(
                branch.BranchId,
                CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(branch.BranchId, result.BranchId);
            Assert.Equal(branch.BranchCode, result.BranchCode);
            Assert.Equal(branch.BranchName, result.BranchName);
            Assert.Equal(branch.Address, result.Address);
            Assert.Equal(branch.City, result.City);
            Assert.Equal(branch.Province, result.Province);
            Assert.Equal(branch.ContactNumber, result.ContactNumber);
            Assert.Equal(branch.TimeZoneId, result.TimeZoneId);
            Assert.Equal(branch.IsActive, result.IsActive);
        }

        [Fact]
        public async Task GetBranchByIdAsync_WhenBranchDoesNotExist_ReturnsNull()
        {
            // Arrange
            var branchId = Guid.NewGuid();

            _branchRepositoryMock
                .Setup(x => x.GetByIdAsync(
                    branchId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((BranchEntity?)null);

            // Act
            var result = await _sut.GetBranchByIdAsync(
                branchId, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region AddBranchAsync

        [Fact]
        public async Task AddBranchAsync_WhenValidRequest_CreatesBranchSuccessfully()
        {
            // Arrange
            var request = CreateBranchRequest();
            BranchEntity? savedBranch = null;

            _branchRepositoryMock
                .Setup(x => x.AddAsync(
                    It.IsAny<BranchEntity>(),
                    It.IsAny<CancellationToken>()))
                .Callback<BranchEntity, CancellationToken>(
                    (branch, _) => savedBranch = branch)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.AddBranchAsync(
                request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.BranchId);
            Assert.Equal(request.BranchCode, result.BranchCode);
            Assert.Equal(request.BranchName, result.BranchName);
            Assert.True(result.IsActive);

            Assert.NotNull(savedBranch);
            Assert.NotEqual(Guid.Empty, savedBranch.BranchId);
            Assert.True(savedBranch.CreatedDate <= DateTime.UtcNow);

            _branchRepositoryMock.Verify(
                x => x.AddAsync(
                    It.IsAny<BranchEntity>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task AddBranchAsync_WhenRequestContainsSpaces_TrimsValues()
        {
            // Arrange
            var request = CreateBranchRequest();
            request.BranchCode = "  JHB  ";
            request.BranchName = "  Johannesburg Branch  ";
            request.City = "  Johannesburg  ";

            // Act
            var result = await _sut.AddBranchAsync(
                request, CancellationToken.None);

            // Assert
            Assert.Equal("JHB", result.BranchCode);
            Assert.Equal("Johannesburg Branch", result.BranchName);
            Assert.Equal("Johannesburg", result.City);
        }

        [Fact]
        public async Task AddBranchAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            var request = CreateBranchRequest();

            _branchRepositoryMock
                .Setup(x => x.AddAsync(
                    It.IsAny<BranchEntity>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException(
                    "Database insert failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<
                InvalidOperationException>(() =>
                    _sut.AddBranchAsync(
                        request, CancellationToken.None));

            Assert.Equal(
                "Database insert failed", exception.Message);

            _unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        #endregion

        #region UpdateBranchAsync

        [Fact]
        public async Task UpdateBranchAsync_WhenBranchExists_UpdatesSuccessfully()
        {
            // Arrange
            var branch = CreateBranch();

            var request = new UpdateBranchRequest
            {
                BranchCode = "PTA",
                BranchName = "Pretoria Branch",
                Address = "456 Main Road",
                City = "Pretoria",
                Province = "Gauteng",
                ContactNumber = "0121234567",
                TimeZoneId = "South Africa Standard Time",
                IsActive = true
            };

            _branchRepositoryMock
                .Setup(x => x.GetByIdAsync(
                    branch.BranchId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(branch);

            // Act
            var result = await _sut.UpdateBranchAsync(
                branch.BranchId,
                request,
                CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("PTA", result.BranchCode);
            Assert.Equal("Pretoria Branch", result.BranchName);
            Assert.Equal("Pretoria", result.City);
            Assert.NotNull(branch.UpdatedDate);

            _branchRepositoryMock.Verify(
                x => x.UpdateAsync(
                    It.IsAny<BranchEntity>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateBranchAsync_WhenBranchDoesNotExist_ReturnsNull()
        {
            // Arrange
            var branchId = Guid.NewGuid();
            var request = new UpdateBranchRequest();

            _branchRepositoryMock
                .Setup(x => x.GetByIdAsync(
                    branchId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((BranchEntity?)null);

            // Act
            var result = await _sut.UpdateBranchAsync(
                branchId, request, CancellationToken.None);

            // Assert
            Assert.Null(result);

            _branchRepositoryMock.Verify(
                x => x.UpdateAsync(
                    It.IsAny<BranchEntity>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateBranchAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            var branch = CreateBranch();
            var request = new UpdateBranchRequest();

            _branchRepositoryMock
                .Setup(x => x.GetByIdAsync(
                    branch.BranchId,
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException(
                    "Database lookup failed"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.UpdateBranchAsync(
                    branch.BranchId,
                    request,
                    CancellationToken.None));

            _unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        #endregion

        #region GetBranchServicesAsync

        [Fact]
        public async Task GetBranchServicesAsync_WhenServicesExist_ReturnsMappedServices()
        {
            // Arrange
            var branchId = Guid.NewGuid();

            var services = new List<BranchServiceEntity>
            {
                CreateBranchService(branchId),
                CreateBranchService(branchId)
            };

            _branchRepositoryMock
                .Setup(x => x.GetBranchServicesAsync(
                    branchId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(services);

            // Act
            var result = await _sut.GetBranchServicesAsync(
                branchId, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Count);

            Assert.All(result, item =>
            {
                Assert.Equal(branchId, item.BranchId);
                Assert.Equal("Haircut", item.ServiceName);
                Assert.Equal(30, item.DurationMinutes);
                Assert.Equal(150m, item.Price);
                Assert.True(item.IsActive);
            });
        }

        [Fact]
        public async Task GetBranchServicesAsync_WhenNoServices_ReturnsEmptyList()
        {
            // Arrange
            var branchId = Guid.NewGuid();

            _branchRepositoryMock
                .Setup(x => x.GetBranchServicesAsync(
                    branchId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<BranchServiceEntity>());

            // Act
            var result = await _sut.GetBranchServicesAsync(
                branchId, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region AddBranchServiceAsync

        [Fact]
        public async Task AddBranchServiceAsync_WhenValidRequest_AddsService()
        {
            // Arrange
            var branchId = Guid.NewGuid();
            var serviceId = Guid.NewGuid();

            var request = new CreateBranchServiceRequest
            {
                ServiceId = serviceId,
                Price = 250m,
                IsActive = true
            };

            BranchServiceEntity? savedEntity = null;

            _branchRepositoryMock
                .Setup(x => x.AddBranchServiceAsync(
                    It.IsAny<BranchServiceEntity>(),
                    It.IsAny<CancellationToken>()))
                .Callback<BranchServiceEntity, CancellationToken>(
                    (entity, _) => savedEntity = entity)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.AddBranchServiceAsync(
                branchId, request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.BranchServiceId);
            Assert.Equal(branchId, result.BranchId);
            Assert.Equal(serviceId, result.ServiceId);
            Assert.Equal(250m, result.Price);
            Assert.True(result.IsActive);

            Assert.NotNull(savedEntity);
            Assert.Equal(branchId, savedEntity.BranchId);

            _branchRepositoryMock.Verify(
                x => x.AddBranchServiceAsync(
                    It.IsAny<BranchServiceEntity>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task AddBranchServiceAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            var branchId = Guid.NewGuid();

            var request = new CreateBranchServiceRequest
            {
                ServiceId = Guid.NewGuid(),
                Price = 100m,
                IsActive = true
            };

            _branchRepositoryMock
                .Setup(x => x.AddBranchServiceAsync(
                    It.IsAny<BranchServiceEntity>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException(
                    "Failed to insert service"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.AddBranchServiceAsync(
                    branchId,
                    request,
                    CancellationToken.None));

            _unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        #endregion

        #region UpdateBranchServiceAsync

        [Fact]
        public async Task UpdateBranchServiceAsync_WhenExists_UpdatesService()
        {
            // Arrange
            var branchId = Guid.NewGuid();
            var existing = CreateBranchService(branchId);

            var request = new UpdateBranchServiceRequest
            {
                Price = 350m,
                IsActive = false
            };

            _branchRepositoryMock
                .Setup(x => x.GetBranchServiceByIdAsync(
                    existing.BranchServiceId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);

            // Act
            var result = await _sut.UpdateBranchServiceAsync(
                branchId,
                existing.BranchServiceId,
                request,
                CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(350m, result.Price);
            Assert.False(result.IsActive);
            Assert.NotNull(existing.UpdatedDate);

            _branchRepositoryMock.Verify(
                x => x.UpdateBranchServiceAsync(
                    It.IsAny<BranchServiceEntity>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateBranchServiceAsync_WhenServiceDoesNotExist_ReturnsNull()
        {
            // Arrange
            var branchId = Guid.NewGuid();
            var branchServiceId = Guid.NewGuid();

            var request = new UpdateBranchServiceRequest
            {
                Price = 100m,
                IsActive = true
            };

            _branchRepositoryMock
                .Setup(x => x.GetBranchServiceByIdAsync(
                    branchServiceId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((BranchServiceEntity?)null);

            // Act
            var result = await _sut.UpdateBranchServiceAsync(
                branchId,
                branchServiceId,
                request,
                CancellationToken.None);

            // Assert
            Assert.Null(result);

            _unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateBranchServiceAsync_WhenBranchIdDoesNotMatch_ReturnsNull()
        {
            // Arrange
            var requestedBranchId = Guid.NewGuid();
            var existing = CreateBranchService(Guid.NewGuid());

            var request = new UpdateBranchServiceRequest
            {
                Price = 300m,
                IsActive = true
            };

            _branchRepositoryMock
                .Setup(x => x.GetBranchServiceByIdAsync(
                    existing.BranchServiceId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);

            // Act
            var result = await _sut.UpdateBranchServiceAsync(
                requestedBranchId,
                existing.BranchServiceId,
                request,
                CancellationToken.None);

            // Assert
            Assert.Null(result);

            _branchRepositoryMock.Verify(
                x => x.UpdateBranchServiceAsync(
                    It.IsAny<BranchServiceEntity>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateBranchServiceAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            var branchId = Guid.NewGuid();
            var existing = CreateBranchService(branchId);

            var request = new UpdateBranchServiceRequest
            {
                Price = 400m,
                IsActive = false
            };

            _branchRepositoryMock
                .Setup(x => x.GetBranchServiceByIdAsync(
                    existing.BranchServiceId,
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException(
                    "Database connection failed"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.UpdateBranchServiceAsync(
                    branchId,
                    existing.BranchServiceId,
                    request,
                    CancellationToken.None));

            _unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        #endregion

        #region Test Data Helpers

        private static BranchEntity CreateBranch()
        {
            return new BranchEntity
            {
                BranchId = Guid.NewGuid(),
                BranchCode = "JHB",
                BranchName = "Johannesburg Branch",
                Address = "123 Main Road",
                City = "Johannesburg",
                Province = "Gauteng",
                ContactNumber = "0111234567",
                TimeZoneId = "South Africa Standard Time",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };
        }

        private static CreateBranchRequest CreateBranchRequest()
        {
            return new CreateBranchRequest
            {
                BranchCode = "JHB",
                BranchName = "Johannesburg Branch",
                Address = "123 Main Road",
                City = "Johannesburg",
                Province = "Gauteng",
                ContactNumber = "0111234567",
                TimeZoneId = "South Africa Standard Time",
                IsActive = true
            };
        }

        private static BranchServiceEntity CreateBranchService(
            Guid branchId)
        {
            return new BranchServiceEntity
            {
                BranchServiceId = Guid.NewGuid(),
                BranchId = branchId,
                ServiceId = Guid.NewGuid(),
                Price = 150m,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                Service = new Service
                {
                    ServiceId = Guid.NewGuid(),
                    ServiceName = "Haircut",
                    DurationMinutes = 30,
                    IsActive = true
                }
            };
        }

        #endregion
    }
}