using Application.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;

namespace Tests.Application
{
    public class CurrentUserServiceTests
    {
        private static CurrentUserService CreateService(HttpContext? httpContext)
        {
            Mock<IHttpContextAccessor> accessorMock = new Mock<IHttpContextAccessor>();
            accessorMock.Setup(accessor => accessor.HttpContext).Returns(httpContext);

            return new CurrentUserService(accessorMock.Object);
        }

        private static DefaultHttpContext CreateContext(params Claim[] claims)
        {
            ClaimsIdentity identity = new ClaimsIdentity(claims, "Test");
            ClaimsPrincipal principal = new ClaimsPrincipal(identity);

            return new DefaultHttpContext { User = principal };
        }

        // ==================== USER ID ====================

        [Fact]
        public void UserId_ShouldReturnParsedValue_WhenNameIdentifierClaimIsValid()
        {
            // Arrange
            CurrentUserService service = CreateService(CreateContext(new Claim(ClaimTypes.NameIdentifier, "42")));

            // Act
            int result = service.UserId;

            // Assert
            result.Should().Be(42);
        }

        [Fact]
        public void UserId_ShouldReturnZero_WhenNameIdentifierClaimIsNotNumeric()
        {
            // Arrange
            CurrentUserService service = CreateService(CreateContext(new Claim(ClaimTypes.NameIdentifier, "not-a-number")));

            // Act
            int result = service.UserId;

            // Assert
            result.Should().Be(0);
        }

        [Fact]
        public void UserId_ShouldReturnZero_WhenNameIdentifierClaimIsEmpty()
        {
            // Arrange
            CurrentUserService service = CreateService(CreateContext(new Claim(ClaimTypes.NameIdentifier, string.Empty)));

            // Act
            int result = service.UserId;

            // Assert
            result.Should().Be(0);
        }

        [Fact]
        public void UserId_ShouldReturnZero_WhenNoNameIdentifierClaimExists()
        {
            // Arrange
            CurrentUserService service = CreateService(CreateContext(new Claim(ClaimTypes.Name, "Juan")));

            // Act
            int result = service.UserId;

            // Assert
            result.Should().Be(0);
        }

        [Fact]
        public void UserId_ShouldReturnZero_WhenHttpContextIsNull()
        {
            // Arrange
            CurrentUserService service = CreateService(null);

            // Act
            int result = service.UserId;

            // Assert
            result.Should().Be(0);
        }

        // ==================== IS AUTHENTICATED ====================

        [Fact]
        public void IsAuthenticated_ShouldReturnTrue_WhenUserIsAuthenticated()
        {
            // Arrange
            CurrentUserService service = CreateService(CreateContext(new Claim(ClaimTypes.NameIdentifier, "1")));

            // Act
            bool result = service.IsAuthenticated;

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsAuthenticated_ShouldReturnFalse_WhenUserIsNotAuthenticated()
        {
            // Arrange
            ClaimsPrincipal anonymousPrincipal = new ClaimsPrincipal(new ClaimsIdentity());

            CurrentUserService service = CreateService(new DefaultHttpContext { User = anonymousPrincipal });

            // Act
            bool result = service.IsAuthenticated;

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void IsAuthenticated_ShouldReturnFalse_WhenHttpContextIsNull()
        {
            // Arrange
            CurrentUserService service = CreateService(null);

            // Act
            bool result = service.IsAuthenticated;

            // Assert
            result.Should().BeFalse();
        }

        // ==================== USER NAME ====================

        [Fact]
        public void UserName_ShouldReturnClaimValue_WhenNameClaimExists()
        {
            // Arrange
            CurrentUserService service = CreateService(CreateContext(new Claim(ClaimTypes.Name, "Juan")));

            // Act
            string? result = service.UserName;

            // Assert
            result.Should().Be("Juan");
        }

        [Fact]
        public void UserName_ShouldReturnNull_WhenNoNameClaimExists()
        {
            // Arrange
            CurrentUserService service = CreateService(CreateContext(new Claim(ClaimTypes.Email, "juan@email.com")));

            // Act
            string? result = service.UserName;

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void UserName_ShouldReturnNull_WhenHttpContextIsNull()
        {
            // Arrange
            CurrentUserService service = CreateService(null);

            // Act
            string? result = service.UserName;

            // Assert
            result.Should().BeNull();
        }

        // ==================== EMAIL ====================

        [Fact]
        public void Email_ShouldReturnClaimValue_WhenEmailClaimExists()
        {
            // Arrange
            CurrentUserService service = CreateService(CreateContext(new Claim(ClaimTypes.Email, "juan@email.com")));

            // Act
            string? result = service.Email;

            // Assert
            result.Should().Be("juan@email.com");
        }

        [Fact]
        public void Email_ShouldReturnNull_WhenNoEmailClaimExists()
        {
            // Arrange
            CurrentUserService service = CreateService(CreateContext(new Claim(ClaimTypes.Name, "Juan")));

            // Act
            string? result = service.Email;

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void Email_ShouldReturnNull_WhenHttpContextIsNull()
        {
            // Arrange
            CurrentUserService service = CreateService(null);

            // Act
            string? result = service.Email;

            // Assert
            result.Should().BeNull();
        }
    }
}
