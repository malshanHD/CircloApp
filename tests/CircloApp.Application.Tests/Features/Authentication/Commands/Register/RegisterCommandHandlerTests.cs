using CircloApp.Application.Common.Constants;
using CircloApp.Application.Common.Models;
using CircloApp.Application.Exceptions;
using CircloApp.Application.Features.Authentication.Commands;
using CircloApp.Application.Features.Authentication.DTOs;
using CircloApp.Application.Interfaces;
using Moq;

namespace CircloApp.Application.Tests.Features.Authentication.Commands.Register
{
    public class RegisterCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock = new();
        private readonly Mock<ICacheService> _cacheServiceMock = new();
        private readonly Mock<IEmailService> _emailServiceMock = new();
        private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
        private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IOtpGenerator> _otpGeneratorMock = new();

        private readonly DateTime _utcNow = new(2026, 8, 3, 12, 0, 0, DateTimeKind.Utc);

        public RegisterCommandHandlerTests()
        {
            _dateTimeProviderMock.Setup(x => x.UtcNow).Returns(_utcNow);
            _otpGeneratorMock.Setup(x => x.GenerateOtp(It.IsAny<int>())).Returns("123456");
        }

        [Fact]
        public async Task Handle_Should_Register_User_When_Request_Is_Valid()
        {
            // Arrange
            SetupUserDoesExist();
            SetupPasswordAndOtpHashing("hashedPassword", "hashedOtp");

            var handler = CreateHandler();
            var request = CreateValidRequest();
            var command = new RegisterUserCommand(request);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert - Response
            Assert.True(result.Success);
            Assert.True(result.RequiresOtpVerification);
            Assert.Equal(request.Email, result.Email);
            Assert.Equal("OTP has been sent to your email.", result.Message);

            // Assert - Verification Helpers
            VerifyUserExistenceChecked(request.Email, request.Username);
            VerifyPendingRegistrationCached(request, "hashedPassword", "hashedOtp");
            _emailServiceMock.Verify(x => x.SendOtpAsync(request.Email, request.FirstName, "123456"), Times.Once);
        }

        [Fact]
        public async Task Handle_GivenEmailAlreadyExists_ShouldReturnFailureResult()
        {
            // Arrange
            SetupUserDoesExist(emailExists: true);
            var handler = CreateHandler();
            var request = CreateValidRequest();
            var command = new RegisterUserCommand(request);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
                handler.Handle(command, CancellationToken.None)
            );

            // Verify exception message
            Assert.Equal("User with the specified email already exists.", exception.Message);

            // Verify side effects were prevented (OTP was NOT sent and Redis was NOT set)
            _cacheServiceMock.Verify(x =>
                x.SetAsync(It.IsAny<string>(), It.IsAny<PendingRegistration>(), It.IsAny<TimeSpan?>()),
                Times.Never
            );

            _emailServiceMock.Verify(x =>
                x.SendOtpAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never
            );
        }

        [Fact]
        public async Task Handle_GivenUsernameAlreadyExists_ShouldReturnFailureResult()
        {
            // Arrange
            SetupUserDoesExist(usernameExists: true);
            var handler = CreateHandler();
            var request = CreateValidRequest();
            var command = new RegisterUserCommand(request);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
                handler.Handle(command, CancellationToken.None)
            );

            // Verify exception message
            Assert.Equal("User with the specified username already exists.", exception.Message);

            // Verify side effects were prevented (OTP was NOT sent and Redis was NOT set)
            _cacheServiceMock.Verify(x =>
                x.SetAsync(It.IsAny<string>(), It.IsAny<PendingRegistration>(), It.IsAny<TimeSpan?>()),
                Times.Never
            );

            _emailServiceMock.Verify(x =>
                x.SendOtpAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never
            );
        }

        #region Helper Methods (Factories & Setups)

        private RegisterUserCommandHandler CreateHandler()
        {
            return new RegisterUserCommandHandler(
                _userRepositoryMock.Object,
                _passwordHasherMock.Object,
                _unitOfWorkMock.Object,
                _cacheServiceMock.Object,
                _emailServiceMock.Object,
                _otpGeneratorMock.Object,
                _dateTimeProviderMock.Object
            );
        }

        // Factory for test data
        private static RegisterUserRequest CreateValidRequest() => new()
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            ContactNumber = "1234567890",
            Username = "johndoe",
            Password = "password123"
        };

        // Reusable mock setups
        private void SetupUserDoesExist(bool emailExists = false, bool usernameExists = false)
        {
            _userRepositoryMock.Setup(x => x.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(emailExists);
            _userRepositoryMock.Setup(x => x.ExistsByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(usernameExists);
        }

        private void SetupPasswordAndOtpHashing(string passHash, string otpHash)
        {
            _passwordHasherMock
                .SetupSequence(x => x.HashPassword(It.IsAny<string>()))
                .Returns(passHash)
                .Returns(otpHash);
        }

        // Reusable assertion verifications
        private void VerifyUserExistenceChecked(string email, string username)
        {
            _userRepositoryMock.Verify(x => x.ExistsByEmailAsync(email, It.IsAny<CancellationToken>()), Times.Once);
            _userRepositoryMock.Verify(x => x.ExistsByUsernameAsync(username, It.IsAny<CancellationToken>()), Times.Once);
        }

        private void VerifyPendingRegistrationCached(RegisterUserRequest request, string passwordHash, string otpHash)
        {
            _cacheServiceMock.Verify(x =>
                x.SetAsync(
                    RedisKeys.Registration(request.Email),
                    It.Is<PendingRegistration>(p =>
                        p.Email == request.Email &&
                        p.FirstName == request.FirstName &&
                        p.Username == request.Username &&
                        p.PasswordHash == passwordHash &&
                        p.OtpHash == otpHash &&
                        p.CreatedAt == _utcNow &&
                        p.OtpExpiresAt == _utcNow.AddMinutes(5)
                    ),
                    It.IsAny<TimeSpan?>()
                ),
                Times.Once);
        }

        #endregion
    }
}
