using CircloApp.Application.Common.Constants;
using CircloApp.Application.Common.Models;
using CircloApp.Application.Exceptions;
using CircloApp.Application.Features.Authentication.DTOs;
using CircloApp.Application.Interfaces;
using CircloApp.Domain.Entities;
using CircloApp.Domain.Enums;
using MediatR;

namespace CircloApp.Application.Features.Authentication.Commands.VerifyOtp
{
    public class VerifyOtpCommandHandler : IRequestHandler<VerifyOtpCommand, VerifyOtpResponse>
    {
        ICacheService _cacheService;
        IUserRepository _userRepository;
        IUnitOfWork _unitOfWork;
        IPasswordHasher _passwordHasher;
        IDateTimeProvider _timeProvider;

        public VerifyOtpCommandHandler(ICacheService cacheService, IUserRepository userRepository, IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, IDateTimeProvider timeProvider)
        {
            _cacheService = cacheService;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _timeProvider = timeProvider;
        }

        public async Task<VerifyOtpResponse> Handle(VerifyOtpCommand command, CancellationToken cancellationToken)
        {
            var pendingUser = await _cacheService.GetAsync<PendingRegistration>(RedisKeys.Registration(command.Request.Email));

            if (pendingUser == null)
            {
                throw new BadRequestException("OTP Expired or Invalid");
            }

            if (pendingUser.FailedAttempts >= 5)
            {
                await RemoveCache(command.Request.Email);
                throw new BadRequestException("Too many failed attempts. Please request a new OTP.");
            }

            var isValid = _passwordHasher.VerifyPassword(command.Request.Otp, pendingUser.OtpHash);

            if (!isValid)
            {
                pendingUser.FailedAttempts++;
                var remaining = pendingUser.OtpExpiresAt - _timeProvider.UtcNow;
                await _cacheService.SetAsync(RedisKeys.Registration(command.Request.Email), pendingUser, remaining);
                throw new BadRequestException("Invalid OTP");
            }

            var emailExists = await _userRepository.ExistsByEmailAsync(pendingUser.Email, cancellationToken);
            if (emailExists)
            {
                await RemoveCache(command.Request.Email);
                throw new BadRequestException("Email already exists");
            }

            var usernameExists = await _userRepository.ExistsByUsernameAsync(pendingUser.Username, cancellationToken);
            if (usernameExists)
            {
                await RemoveCache(command.Request.Email);
                throw new BadRequestException("Username already exists");
            }

            var user = new User
            {
                FirstName = pendingUser.FirstName,
                LastName = pendingUser.LastName,
                Email = pendingUser.Email,
                ContactNumber = pendingUser.ContactNumber,
                Username = pendingUser.Username,
                PasswordHash = pendingUser.PasswordHash,
                Role = UserRole.User
            };

            await _userRepository.AddAsync(user, cancellationToken: cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await RemoveCache(command.Request.Email);

            return new VerifyOtpResponse
            {
                Success = true,
                Message = "Email Verified Successfully"
            };
        }

        private async Task RemoveCache(string email)
        {
            await _cacheService.RemoveAsync(RedisKeys.Registration(email));
        }
    }
}
