using Application.DTOs.Request;
using Application.DTOs.Response;
using Application.Interfaces;
using Application.Mappings;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces.Repositories;
using Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Application
{
    public class CategoryServiceTests
    {
        private readonly Mock<ICategoryRepository> _mockRepository;
        private readonly IMapper _mapper;
        private readonly ICategoryService _service;

        public CategoryServiceTests()
        {
            _mockRepository = new Mock<ICategoryRepository>();

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

            _service = new CategoryService(_mockRepository.Object, _mapper);
        }

        // ==================== PRUEBAS DE CONSULTAS ====================

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCategory_WhenCategoryExists()
        {
            // Arrange
            int categoryId = 1;
            Category expectedCategory = new Category(new EntityInfo( "Test Category", "Test Description"));
            // Simular que el repositorio devuelve la categoría
            _mockRepository
                .Setup(repo => repo.GetByIdAsync(categoryId))
                .ReturnsAsync(expectedCategory);

            // Act
            CategoryResponseDto result = await _service.GetByIdAsync(categoryId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(expectedCategory.Id);
            result.Name.Should().Be(expectedCategory.Info.Name);
            result.Description.Should().Be(expectedCategory.Info.Description);
            result.IsActive.Should().Be(expectedCategory.IsActive);

            _mockRepository.Verify(repo => repo.GetByIdAsync(categoryId), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldThrowKeyNotFoundException_WhenCategoryNotFound()
        {
            // Arrange
            int categoryId = 999;
            _mockRepository
                .Setup(repo => repo.GetByIdAsync(categoryId))
                .ReturnsAsync((Category)null);

            // Act & Assert
            Func<Task> act = async () => await _service.GetByIdAsync(categoryId);
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage($"*{categoryId}*");
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllCategories()
        {
            // Arrange
            List<Category> categories = new List<Category>
            {
                new Category(new EntityInfo("Category 1", "Description 1")),
                new Category(new EntityInfo("Category 2", "Description 2")),
                new Category(new EntityInfo("Category 3", "Description 3"))
            };

            _mockRepository
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(categories);

            // Act
            List<CategoryResponseDto> result = await _service.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(categories.Count);
            result.Select(dto => dto.Name).Should().Contain(new[] { "Category 1", "Category 2", "Category 3" });
            _mockRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetActiveCategoriesAsync_ShouldReturnOnlyActiveCategories()
        {
            // Arrange
            List<Category> categories = new List<Category>
            {
                new Category(new EntityInfo("Active 1", "Desc")),
                new Category(new EntityInfo("Active 2", "Desc"))
            };

            // Simular que solo devuelve las activas
            _mockRepository
                .Setup(repo => repo.GetActiveCategoriesAsync())
                .ReturnsAsync(categories);

            // Act
            List<CategoryResponseDto> result = await _service.GetActiveCategoriesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(2);
            result.All(dto => dto.IsActive).Should().BeTrue();
            _mockRepository.Verify(repo => repo.GetActiveCategoriesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetByNameAsync_ShouldReturnCategory_WhenNameExists()
        {
            // Arrange
            string categoryName = "Alimentación";
            Category expectedCategory = new Category(new EntityInfo(categoryName, "Gastos de comida"));
            _mockRepository
                .Setup(repo => repo.GetByNameAsync(categoryName))
                .ReturnsAsync(expectedCategory);

            // Act
            CategoryResponseDto result = await _service.GetByNameAsync(categoryName);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(categoryName);
            _mockRepository.Verify(repo => repo.GetByNameAsync(categoryName), Times.Once);
        }

        [Fact]
        public async Task GetByNameAsync_ShouldThrowKeyNotFoundException_WhenNameNotFound()
        {
            // Arrange
            string categoryName = "Inexistente";
            _mockRepository
                .Setup(repo => repo.GetByNameAsync(categoryName))
                .ReturnsAsync((Category)null);

            // Act & Assert
            Func<Task> act = async () => await _service.GetByNameAsync(categoryName);
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage($"*{categoryName}*");
        }

        // ==================== PRUEBAS DE COMANDOS ====================

        [Fact]
        public async Task CreateAsync_ShouldCreateCategory_WhenNameIsUnique()
        {
            // Arrange
            CreateCategoryRequestDto request = new CreateCategoryRequestDto
            {
                Name = "Nueva Categoría",
                Description = "Descripción de la nueva categoría"
            };

            _mockRepository
                .Setup(repo => repo.ExistsByNameAsync(request.Name))
                .ReturnsAsync(false);

            Category createdCategory = null;
            _mockRepository
                .Setup(repo => repo.AddAsync(It.IsAny<Category>()))
                .Callback<Category>(category => createdCategory = category)
                .Returns(Task.CompletedTask);

            // Act
            CategoryResponseDto result = await _service.CreateAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(request.Name);
            result.Description.Should().Be(request.Description);
            result.IsActive.Should().BeTrue();

            _mockRepository.Verify(repo => repo.ExistsByNameAsync(request.Name), Times.Once);
            _mockRepository.Verify(repo => repo.AddAsync(It.IsAny<Category>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowInvalidOperationException_WhenNameAlreadyExists()
        {
            // Arrange
            CreateCategoryRequestDto request = new CreateCategoryRequestDto
            {
                Name = "Categoría Duplicada",
                Description = "Descripción"
            };

            _mockRepository
                .Setup(repo => repo.ExistsByNameAsync(request.Name))
                .ReturnsAsync(true);

            // Act & Assert
            Func<Task> act = async () => await _service.CreateAsync(request);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"*{request.Name}*");

            _mockRepository.Verify(repo => repo.ExistsByNameAsync(request.Name), Times.Once);
            _mockRepository.Verify(repo => repo.AddAsync(It.IsAny<Category>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateCategory_WhenIdExistsAndNameIsUnique()
        {
            // Arrange
            int categoryId = 1;
            Category existingCategory = new Category(new EntityInfo("Nombre Antiguo", "Descripción Antigua"));
            UpdateCategoryRequestDto request = new UpdateCategoryRequestDto
            {
                Name = "Nuevo Nombre",
                Description = "Nueva Descripción"
            };

            _mockRepository
                .Setup(repo => repo.GetByIdAsync(categoryId))
                .ReturnsAsync(existingCategory);

            _mockRepository
                .Setup(repo => repo.GetByNameAsync(request.Name))
                .ReturnsAsync((Category)null);

            // Act
            CategoryResponseDto result = await _service.UpdateAsync(categoryId, request);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(request.Name);
            result.Description.Should().Be(request.Description);

            _mockRepository.Verify(repo => repo.GetByIdAsync(categoryId), Times.Once);
            _mockRepository.Verify(repo => repo.GetByNameAsync(request.Name), Times.Once);
            _mockRepository.Verify(repo => repo.UpdateAsync(existingCategory), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowKeyNotFoundException_WhenCategoryNotFound()
        {
            // Arrange
            int categoryId = 999;
            UpdateCategoryRequestDto request = new UpdateCategoryRequestDto
            {
                Name = "Nuevo Nombre",
                Description = "Nueva Descripción"
            };

            _mockRepository
                .Setup(repo => repo.GetByIdAsync(categoryId))
                .ReturnsAsync((Category)null);

            // Act & Assert
            Func<Task> act = async () => await _service.UpdateAsync(categoryId, request);
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage($"*{categoryId}*");

            _mockRepository.Verify(repo => repo.GetByIdAsync(categoryId), Times.Once);
            _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Category>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowInvalidOperationException_WhenNameAlreadyExistsInOtherCategory()
        {
            // Arrange
            int categoryId = 1;
            Category existingCategory = new Category(new EntityInfo("Nombre Antiguo", "Descripción Antigua"));
            Category otherCategory = new Category(new EntityInfo("Nuevo Nombre", "Otra Descripción"));
            otherCategory.GetType().GetProperty("Id").SetValue(otherCategory, 2); // Simular ID diferente

            UpdateCategoryRequestDto request = new UpdateCategoryRequestDto
            {
                Name = "Nuevo Nombre",
                Description = "Nueva Descripción"
            };

            _mockRepository
                .Setup(repo => repo.GetByIdAsync(categoryId))
                .ReturnsAsync(existingCategory);

            _mockRepository
                .Setup(repo => repo.GetByNameAsync(request.Name))
                .ReturnsAsync(otherCategory);

            // Act & Assert
            Func<Task> act = async () => await _service.UpdateAsync(categoryId, request);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"*{request.Name}*");

            _mockRepository.Verify(repo => repo.GetByIdAsync(categoryId), Times.Once);
            _mockRepository.Verify(repo => repo.GetByNameAsync(request.Name), Times.Once);
            _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Category>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteCategory_WhenIdExistsAndNoExpenses()
        {
            // Arrange
            int categoryId = 1;

            _mockRepository
                .Setup(repo => repo.ExistsAsync(categoryId))
                .ReturnsAsync(true);

            _mockRepository
                .Setup(repo => repo.HasExpensesAsync(categoryId))
                .ReturnsAsync(false);

            // Act
            await _service.DeleteAsync(categoryId);

            // Assert
            _mockRepository.Verify(repo => repo.ExistsAsync(categoryId), Times.Once);
            _mockRepository.Verify(repo => repo.HasExpensesAsync(categoryId), Times.Once);
            _mockRepository.Verify(repo => repo.DeleteAsync(categoryId), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowKeyNotFoundException_WhenCategoryNotFound()
        {
            // Arrange
            int categoryId = 999;

            _mockRepository
                .Setup(repo => repo.ExistsAsync(categoryId))
                .ReturnsAsync(false);

            // Act & Assert
            Func<Task> act = async () => await _service.DeleteAsync(categoryId);
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage($"*{categoryId}*");

            _mockRepository.Verify(repo => repo.ExistsAsync(categoryId), Times.Once);
            _mockRepository.Verify(repo => repo.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowInvalidOperationException_WhenCategoryHasExpenses()
        {
            // Arrange
            int categoryId = 1;

            _mockRepository
                .Setup(repo => repo.ExistsAsync(categoryId))
                .ReturnsAsync(true);

            _mockRepository
                .Setup(repo => repo.HasExpensesAsync(categoryId))
                .ReturnsAsync(true);

            // Act & Assert
            Func<Task> act = async () => await _service.DeleteAsync(categoryId);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"*{categoryId}*");

            _mockRepository.Verify(repo => repo.ExistsAsync(categoryId), Times.Once);
            _mockRepository.Verify(repo => repo.HasExpensesAsync(categoryId), Times.Once);
            _mockRepository.Verify(repo => repo.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        // ==================== PRUEBAS DE VALIDACIONES ====================

        [Fact]
        public async Task ExistsAsync_ShouldReturnTrue_WhenCategoryExists()
        {
            // Arrange
            int categoryId = 1;
            _mockRepository
                .Setup(repo => repo.ExistsAsync(categoryId))
                .ReturnsAsync(true);

            // Act
            bool result = await _service.ExistsAsync(categoryId);

            // Assert
            result.Should().BeTrue();
            _mockRepository.Verify(repo => repo.ExistsAsync(categoryId), Times.Once);
        }

        [Fact]
        public async Task ExistsAsync_ShouldReturnFalse_WhenCategoryDoesNotExist()
        {
            // Arrange
            int categoryId = 999;
            _mockRepository
                .Setup(repo => repo.ExistsAsync(categoryId))
                .ReturnsAsync(false);

            // Act
            bool result = await _service.ExistsAsync(categoryId);

            // Assert
            result.Should().BeFalse();
            _mockRepository.Verify(repo => repo.ExistsAsync(categoryId), Times.Once);
        }

        [Fact]
        public async Task CanDeleteAsync_ShouldReturnTrue_WhenCategoryExistsAndNoExpenses()
        {
            // Arrange
            int categoryId = 1;
            _mockRepository
                .Setup(repo => repo.ExistsAsync(categoryId))
                .ReturnsAsync(true);
            _mockRepository
                .Setup(repo => repo.HasExpensesAsync(categoryId))
                .ReturnsAsync(false);

            // Act
            bool result = await _service.CanDeleteAsync(categoryId);

            // Assert
            result.Should().BeTrue();
            _mockRepository.Verify(repo => repo.ExistsAsync(categoryId), Times.Once);
            _mockRepository.Verify(repo => repo.HasExpensesAsync(categoryId), Times.Once);
        }

        [Fact]
        public async Task CanDeleteAsync_ShouldReturnFalse_WhenCategoryNotExists()
        {
            // Arrange
            int categoryId = 999;
            _mockRepository
                .Setup(repo => repo.ExistsAsync(categoryId))
                .ReturnsAsync(false);

            // Act
            bool result = await _service.CanDeleteAsync(categoryId);

            // Assert
            result.Should().BeFalse();
            _mockRepository.Verify(repo => repo.ExistsAsync(categoryId), Times.Once);
            _mockRepository.Verify(repo => repo.HasExpensesAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task CanDeleteAsync_ShouldReturnFalse_WhenCategoryHasExpenses()
        {
            // Arrange
            int categoryId = 1;
            _mockRepository
                .Setup(repo => repo.ExistsAsync(categoryId))
                .ReturnsAsync(true);
            _mockRepository
                .Setup(repo => repo.HasExpensesAsync(categoryId))
                .ReturnsAsync(true);

            // Act
            bool result = await _service.CanDeleteAsync(categoryId);

            // Assert
            result.Should().BeFalse();
            _mockRepository.Verify(repo => repo.ExistsAsync(categoryId), Times.Once);
            _mockRepository.Verify(repo => repo.HasExpensesAsync(categoryId), Times.Once);
        }
    }
}
