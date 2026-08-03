using CircloApp.Application.Exceptions;
using CircloApp.Application.Features.Authentication.Commands.Login;
using CircloApp.Application.Features.Authentication.DTOs;
using CircloApp.Application.Interfaces;
using CircloApp.Domain.Entities;
using Moq;

namespace CircloApp.Application.Tests.Features.Authentication.Commands.Login
{
    public class LoginCommandHandlerTest
    {
        private readonly Mock<IUserRepository> _userRepositoryMock = new();
        private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
        private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock = new();
        private readonly Mock<IRefreshTokenGenerator> _refreshTokenGeneratorMock = new();

        private readonly DateTime _utcNow = new(2026, 8, 3, 12, 0, 0, DateTimeKind.Utc);

        public LoginCommandHandlerTest()
        {
            _dateTimeProviderMock.Setup(x => x.UtcNow).Returns(_utcNow);
        }

        [Fact]
        public async Task Handle_GivenValidCredentials_ShouldReturnLoginResponseAndSaveRefreshToken()
        {
            // Arrange
            var user = CreateTestUser();
            var request = CreateLoginRequest();
            var command = new LoginCommand(request);

            _userRepositoryMock
                .Setup(x => x.GetByUsernameOrEmailAsync(request.UsernameOrEmail))
                .ReturnsAsync(user);

            _passwordHasherMock
                .Setup(x => x.VerifyPassword(request.Password, user.PasswordHash))
                .Returns(true);

            _jwtTokenGeneratorMock
                .Setup(x => x.GenerateToken(user))
                .Returns("valid-access-token");

            _refreshTokenGeneratorMock
                .Setup(x => x.Generate())
                .Returns("valid-refresh-token");

            var handler = CreateHandler();

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert - Response Data
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.UserId);
            Assert.Equal(user.Username, result.Username);
            Assert.Equal(user.Email, result.Email);
            Assert.Equal("valid-access-token", result.AccessToken);
            Assert.Equal("valid-refresh-token", result.RefreshToken);
            Assert.Equal(_utcNow.AddMinutes(20), result.ExpiresAt);

            // Assert - State & Side Effects
            Assert.Equal("valid-refresh-token", user.RefreshToken);
            Assert.Equal(_utcNow.AddDays(7), user.RefreshTokenExpiryTime);

            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_GivenUserDoesNotExist_ShouldThrowBadRequestException()
        {
            // Arrange
            var request = CreateLoginRequest();
            var command = new LoginCommand(request);

            _userRepositoryMock
                .Setup(x => x.GetByUsernameOrEmailAsync(request.UsernameOrEmail))
                .ReturnsAsync((User?)null);

            var handler = CreateHandler();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
                handler.Handle(command, CancellationToken.None));

            Assert.Equal("Invalid username/email or password.", exception.Message);

            // Verify no tokens generated and unit of work never saved changes
            _jwtTokenGeneratorMock.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_GivenInvalidPassword_ShouldThrowBadRequestException()
        {
            // Arrange
            var user = CreateTestUser();
            var request = CreateLoginRequest(password: "wrongpassword");
            var command = new LoginCommand(request);

            _userRepositoryMock
                .Setup(x => x.GetByUsernameOrEmailAsync(request.UsernameOrEmail))
                .ReturnsAsync(user);

            _passwordHasherMock
                .Setup(x => x.VerifyPassword(request.Password, user.PasswordHash))
                .Returns(false);

            var handler = CreateHandler();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
                handler.Handle(command, CancellationToken.None));

            Assert.Equal("Invalid username/email or password.", exception.Message);

            // Verify no token generation or database commit happened
            _jwtTokenGeneratorMock.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        #region Helper Methods (Factories & Setups)
        private LoginCommandHandler CreateHandler()
        {
            return new LoginCommandHandler(
                _userRepositoryMock.Object,
                _passwordHasherMock.Object,
                _dateTimeProviderMock.Object,
                _jwtTokenGeneratorMock.Object,
                _unitOfWorkMock.Object,
                _refreshTokenGeneratorMock.Object
            );
        }

        private static LoginRequest CreateLoginRequest(string usernameOrEmail = "johndoe", string password = "password123")
        {
            return new LoginRequest
            {
                UsernameOrEmail = usernameOrEmail,
                Password = password
            };
        }

        private static User CreateTestUser()
        {
            return new User
            {
                Id = Guid.NewGuid(),
                Username = "johndoe",
                Email = "john.doe@example.com",
                PasswordHash = "hashedPassword123"
            };
        }
        #endregion
    }
}
