using Application.DTOs.Request;
using Application.DTOs.Response;
using Application.Interfaces;
using Application.Mappings;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Helpers;

namespace Tests.Application
{
    public class CategoryServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly Mock<ICategoryRepository> _mockRepository;
        private readonly IMapper _mapper;
        private readonly ICategoryService _service;

        public CategoryServiceTests()
        {
            // Configurar AutoMapper
            MapperConfiguration mapperConfiguration = new MapperConfiguration
            (
                config =>
                {
                    config.AddProfile<AutoMapperProfile>();
                },
                new LoggerFactory()
            );

            _mapper = mapperConfiguration.CreateMapper();

            // Crear mocks
            _userRepositoryMock = new Mock<IUserRepository>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();
            _mockRepository = new Mock<ICategoryRepository>();

            // Instanciar el servicio
            _service = new CategoryService(_currentUserServiceMock.Object, _mockRepository.Object, _userRepositoryMock.Object, _mapper);
        }

        // ==================== PRUEBAS DE CONSULTAS ====================

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCategory_WhenCategoryExists()
        {
            // Arrange
            int userId = 1;
            int categoryId = 1;
            User user = TestDataFactory.CreateUser(userId);
            Category expectedCategory = TestDataFactory.CreateCategory(categoryId, user);

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            // Simular que el repositorio devuelve la categoría
            _mockRepository
                .Setup(repo => repo.GetByIdAsync(categoryId, userId))
                .ReturnsAsync(expectedCategory);

            // Act
            CategoryResponseDTO result = await _service.GetByIdAsync(categoryId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(expectedCategory.Id);
            result.Name.Should().Be(expectedCategory.Info.Name);
            result.Description.Should().Be(expectedCategory.Info.Description);
            result.IsActive.Should().Be(expectedCategory.IsActive);

            _mockRepository.Verify(repo => repo.GetByIdAsync(categoryId, userId), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldThrowKeyNotFoundException_WhenCategoryNotFound()
        {
            // Arrange
            int userId = 1;
            int categoryId = 999;
            User user = TestDataFactory.CreateUser(userId);

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _mockRepository
                .Setup(repo => repo.GetByIdAsync(categoryId, userId))
                .ReturnsAsync((Category?)null);

            // Act & Assert
            Func<Task> act = async () => await _service.GetByIdAsync(categoryId);

            await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage($"*{categoryId}*");
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllCategories()
        {
            // Arrange
            int userId = 1;
            User user = TestDataFactory.CreateUser(userId);

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            List<Category> categories = TestDataFactory.CreateCategories();

            _mockRepository
                .Setup(repo => repo.GetAllAsync(userId))
                .ReturnsAsync(categories);

            // Act
            List<CategoryResponseDTO> result = await _service.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(categories.Count);
            result.Select(dto => dto.Name).Should().Contain([$"{TestDataFactory.DEFAULT_CATEGORIES_NAME} 1", $"{TestDataFactory.DEFAULT_CATEGORIES_NAME} 2", $"{TestDataFactory.DEFAULT_CATEGORIES_NAME} 3"]);
            _mockRepository.Verify(repo => repo.GetAllAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetActiveCategoriesAsync_ShouldReturnOnlyActiveCategories()
        {
            // Arrange
            int userId = 1;
            User user = TestDataFactory.CreateUser(userId);

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            List<Category> categories = TestDataFactory.CreateCategories(2);

            // Simular que solo devuelve las activas
            _mockRepository
                .Setup(repo => repo.GetActiveCategoriesAsync(userId))
                .ReturnsAsync(categories);

            // Act
            List<CategoryResponseDTO> result = await _service.GetActiveCategoriesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(categories.Count);
            result.All(dto => dto.IsActive).Should().BeTrue();
            _mockRepository.Verify(repo => repo.GetActiveCategoriesAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetByNameAsync_ShouldReturnCategory_WhenNameExists()
        {
            // Arrange
            int userId = 1;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _mockRepository
                .Setup(repo => repo.GetByNameAsync(TestDataFactory.DEFAULT_ENTITY_INFO_NAME, userId))
                .ReturnsAsync(TestDataFactory.CreateCategory(name: TestDataFactory.DEFAULT_ENTITY_INFO_NAME));

            // Act
            CategoryResponseDTO result = await _service.GetByNameAsync(TestDataFactory.DEFAULT_ENTITY_INFO_NAME);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(TestDataFactory.DEFAULT_ENTITY_INFO_NAME);
            _mockRepository.Verify(repo => repo.GetByNameAsync(TestDataFactory.DEFAULT_ENTITY_INFO_NAME, userId), Times.Once);
        }

        [Fact]
        public async Task GetByNameAsync_ShouldThrowKeyNotFoundException_WhenNameNotFound()
        {
            // Arrange
            int userId = 1;
            string categoryName = "Inexistente";

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _mockRepository
                .Setup(repo => repo.GetByNameAsync(categoryName, userId))
                .ReturnsAsync((Category?)null);

            // Act & Assert
            Func<Task> act = async () => await _service.GetByNameAsync(categoryName);
            await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage($"*{categoryName}*");
        }

        // ==================== PRUEBAS DE COMANDOS ====================

        [Fact]
        public async Task CreateAsync_ShouldCreateCategory_WhenNameIsUnique()
        {
            // Arrange
            int userId = 1;
            User user = TestDataFactory.CreateUser(userId);

            CreateCategoryRequestDTO request = new CreateCategoryRequestDTO
            {
                Name = "Nueva Categoría",
                Description = "Descripción de la nueva categoría"
            };

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _mockRepository
                .Setup(repo => repo.ExistsByNameAsync(request.Name, userId))
                .ReturnsAsync(false);

            _userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId, It.IsAny<bool>()))
                .ReturnsAsync(user);

            Category? createdCategory = null;

            _mockRepository
                .Setup(repo => repo.AddAsync(It.IsAny<Category>()))
                .Callback<Category>(category => createdCategory = category)
                .Returns(Task.CompletedTask);

            // Act
            CategoryResponseDTO result = await _service.CreateAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(request.Name);
            result.Description.Should().Be(request.Description);
            result.IsActive.Should().BeTrue();

            _mockRepository.Verify(repo => repo.ExistsByNameAsync(request.Name, userId), Times.Once);
            _mockRepository.Verify(repo => repo.AddAsync(It.IsAny<Category>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowInvalidOperationException_WhenNameAlreadyExists()
        {
            // Arrange
            int userId = 1;

            CreateCategoryRequestDTO request = new CreateCategoryRequestDTO
            {
                Name = "Categoría Duplicada",
                Description = "Descripción"
            };

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _mockRepository
                .Setup(repo => repo.ExistsByNameAsync(request.Name, userId))
                .ReturnsAsync(true);

            // Act & Assert
            Func<Task> act = async () => await _service.CreateAsync(request);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage($"*{request.Name}*");

            _mockRepository.Verify(repo => repo.ExistsByNameAsync(request.Name, userId), Times.Once);
            _mockRepository.Verify(repo => repo.AddAsync(It.IsAny<Category>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateCategory_WhenIdExistsAndNameIsUnique()
        {
            // Arrange
            int userId = 1;
            int categoryId = 1;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            Category existingCategory = TestDataFactory.CreateCategory(1, TestDataFactory.CreateUser(), TestDataFactory.CreateEntityInfo("Nombre Antiguo", "Descripción Antigua"));

            UpdateCategoryRequestDTO request = new UpdateCategoryRequestDTO
            {
                Name = "Nuevo Nombre",
                Description = "Nueva Descripción"
            };

            _mockRepository
                .Setup(repo => repo.GetByIdAsync(categoryId, userId))
                .ReturnsAsync(existingCategory);

            _mockRepository
                .Setup(repo => repo.GetByNameAsync(request.Name, userId))
                .ReturnsAsync((Category?)null);

            // Act
            CategoryResponseDTO result = await _service.UpdateAsync(categoryId, request);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(request.Name);
            result.Description.Should().Be(request.Description);

            _mockRepository.Verify(repo => repo.GetByIdAsync(categoryId, userId), Times.Once);
            _mockRepository.Verify(repo => repo.GetByNameAsync(request.Name, userId), Times.Once);
            _mockRepository.Verify(repo => repo.UpdateAsync(existingCategory), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowKeyNotFoundException_WhenCategoryNotFound()
        {
            // Arrange
            int userId = 1;
            int categoryId = 999;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _mockRepository
                .Setup(repo => repo.GetByIdAsync(categoryId, userId))
                .ReturnsAsync((Category?)null);

            // Act & Assert
            Func<Task> act = async () => await _service.UpdateAsync
            (
                categoryId,
                new UpdateCategoryRequestDTO
                {
                    Name = "Nuevo Nombre",
                    Description = "Nueva Descripción"
                }
            );

            await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage($"*{categoryId}*");

            _mockRepository.Verify(repo => repo.GetByIdAsync(categoryId, userId), Times.Once);
            _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Category>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowInvalidOperationException_WhenNameAlreadyExistsInOtherCategory()
        {
            // Arrange
            int userId = 1;
            int categoryId = 1;

            User user = TestDataFactory.CreateUser(userId);
            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            UpdateCategoryRequestDTO request = new UpdateCategoryRequestDTO
            {
                Name = "Nuevo Nombre",
                Description = "Nueva Descripción"
            };

            _mockRepository
                .Setup(repo => repo.GetByIdAsync(categoryId, userId))
                .ReturnsAsync(TestDataFactory.CreateCategory(1, user, "Nombre Antiguo", "Descripción Antigua"));

            _mockRepository
                .Setup(repo => repo.GetByNameAsync(request.Name, userId))
                .ReturnsAsync(TestDataFactory.CreateCategory(2, user, "Nuevo Nombre", "Otra Descripción"));

            // Act & Assert
            Func<Task> act = async () => await _service.UpdateAsync(categoryId, request);
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage($"*{request.Name}*");

            _mockRepository.Verify(repo => repo.GetByIdAsync(categoryId, userId), Times.Once);
            _mockRepository.Verify(repo => repo.GetByNameAsync(request.Name, userId), Times.Once);
            _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Category>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteCategory_WhenIdExistsAndNoExpenses()
        {
            // Arrange
            int userId = 1;
            int categoryId = 1;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _mockRepository
                .Setup(repo => repo.ExistsAsync(categoryId, userId))
                .ReturnsAsync(true);

            _mockRepository
                .Setup(repo => repo.HasDependenciesAsync(categoryId, userId))
                .ReturnsAsync(false);

            // Act
            await _service.DeleteAsync(categoryId);

            // Assert
            _mockRepository.Verify(repo => repo.ExistsAsync(categoryId, userId), Times.Once);
            _mockRepository.Verify(repo => repo.HasDependenciesAsync(categoryId, userId), Times.Once);
            _mockRepository.Verify(repo => repo.DeleteAsync(categoryId, userId), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowKeyNotFoundException_WhenCategoryNotFound()
        {
            // Arrange
            int userId = 1;
            int categoryId = 999;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _mockRepository
                .Setup(repo => repo.ExistsAsync(categoryId, userId))
                .ReturnsAsync(false);

            // Act & Assert
            Func<Task> act = async () => await _service.DeleteAsync(categoryId);
            await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage($"*{categoryId}*");

            _mockRepository.Verify(repo => repo.ExistsAsync(categoryId, userId), Times.Once);
            _mockRepository.Verify(repo => repo.DeleteAsync(It.IsAny<int>(), userId), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowInvalidOperationException_WhenCategoryHasExpenses()
        {
            // Arrange
            int userId = 1;
            int categoryId = 1;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _mockRepository
                .Setup(repo => repo.ExistsAsync(categoryId, userId))
                .ReturnsAsync(true);

            _mockRepository
                .Setup(repo => repo.HasDependenciesAsync(categoryId, userId))
                .ReturnsAsync(true);

            // Act & Assert
            Func<Task> act = async () => await _service.DeleteAsync(categoryId);
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage($"*{categoryId}*");

            _mockRepository.Verify(repo => repo.ExistsAsync(categoryId, userId), Times.Once);
            _mockRepository.Verify(repo => repo.HasDependenciesAsync(categoryId, userId), Times.Once);
            _mockRepository.Verify(repo => repo.DeleteAsync(It.IsAny<int>(), userId), Times.Never);
        }

        // ==================== PRUEBAS DE VALIDACIONES ====================

        [Fact]
        public async Task ExistsAsync_ShouldReturnTrue_WhenCategoryExists()
        {
            // Arrange
            int userId = 1;
            int categoryId = 1;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _mockRepository
                .Setup(repo => repo.ExistsAsync(categoryId, userId))
                .ReturnsAsync(true);

            // Act
            bool result = await _service.ExistsAsync(categoryId);

            // Assert
            result.Should().BeTrue();
            _mockRepository.Verify(repo => repo.ExistsAsync(categoryId, userId), Times.Once);
        }

        [Fact]
        public async Task ExistsAsync_ShouldReturnFalse_WhenCategoryDoesNotExist()
        {
            // Arrange
            int userId = 1;
            int categoryId = 999;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _mockRepository
                .Setup(repo => repo.ExistsAsync(categoryId, userId))
                .ReturnsAsync(false);

            // Act
            bool result = await _service.ExistsAsync(categoryId);

            // Assert
            result.Should().BeFalse();
            _mockRepository.Verify(repo => repo.ExistsAsync(categoryId, userId), Times.Once);
        }

        [Fact]
        public async Task CanDeleteAsync_ShouldReturnTrue_WhenCategoryExistsAndNoExpenses()
        {
            // Arrange
            int userId = 1;
            int categoryId = 1;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _mockRepository
                .Setup(repo => repo.ExistsAsync(categoryId, userId))
                .ReturnsAsync(true);
            _mockRepository
                .Setup(repo => repo.HasDependenciesAsync(categoryId, userId))
                .ReturnsAsync(false);

            // Act
            bool result = await _service.CanDeleteAsync(categoryId);

            // Assert
            result.Should().BeTrue();
            _mockRepository.Verify(repo => repo.ExistsAsync(categoryId, userId), Times.Once);
            _mockRepository.Verify(repo => repo.HasDependenciesAsync(categoryId, userId), Times.Once);
        }

        [Fact]
        public async Task CanDeleteAsync_ShouldReturnFalse_WhenCategoryNotExists()
        {
            // Arrange
            int userId = 1;
            int categoryId = 999;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _mockRepository
                .Setup(repo => repo.ExistsAsync(categoryId, userId))
                .ReturnsAsync(false);

            // Act
            bool result = await _service.CanDeleteAsync(categoryId);

            // Assert
            result.Should().BeFalse();
            _mockRepository.Verify(repo => repo.ExistsAsync(categoryId, userId), Times.Once);
            _mockRepository.Verify(repo => repo.HasDependenciesAsync(It.IsAny<int>(), userId), Times.Never);
        }

        [Fact]
        public async Task CanDeleteAsync_ShouldReturnFalse_WhenCategoryHasExpenses()
        {
            // Arrange
            int userId = 1;
            int categoryId = 1;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _mockRepository
                .Setup(repo => repo.ExistsAsync(categoryId, userId))
                .ReturnsAsync(true);
            _mockRepository
                .Setup(repo => repo.HasDependenciesAsync(categoryId, userId))
                .ReturnsAsync(true);

            // Act
            bool result = await _service.CanDeleteAsync(categoryId);

            // Assert
            result.Should().BeFalse();
            _mockRepository.Verify(repo => repo.ExistsAsync(categoryId, userId), Times.Once);
            _mockRepository.Verify(repo => repo.HasDependenciesAsync(categoryId, userId), Times.Once);
        }
    }
}