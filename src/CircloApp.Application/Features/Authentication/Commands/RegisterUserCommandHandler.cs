using CircloApp.Application.Common.Constants;
using CircloApp.Application.Common.Models;
using CircloApp.Application.Exceptions;
using CircloApp.Application.Features.Authentication.DTOs;
using CircloApp.Application.Interfaces;
using CircloApp.Domain.Entities;
using MediatR;

namespace CircloApp.Application.Features.Authentication.Commands
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly IEmailService _emailService;
        private readonly IOtpGenerator _otpGenerator;
        private readonly IDateTimeProvider _dateTimeProvider;

        public RegisterUserCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, IUnitOfWork unitOfWork, 
                                            ICacheService cacheService, IEmailService emailService, IOtpGenerator otpGenerator, IDateTimeProvider dateTimeProvider)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
            _emailService = emailService;
            _otpGenerator = otpGenerator;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<RegisterResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Request;
            var emailExists = await _userRepository.ExistsByEmailAsync(dto.Email, cancellationToken);

            if (emailExists)
            {
                throw new BadRequestException("User with the specified email already exists.");
            }

            var userNameExists = await _userRepository.ExistsByUsernameAsync(dto.Username, cancellationToken);
            if (userNameExists)
            {
                throw new BadRequestException("User with the specified username already exists.");
            }

            var hashPassword = _passwordHasher.HashPassword(dto.Password);
            var otp = _otpGenerator.GenerateOtp(6);
            var otphash = _passwordHasher.HashPassword(otp);

            var pending = new PendingRegistration
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                ContactNumber = dto.ContactNumber,
                Username = dto.Username,
                PasswordHash = hashPassword,
                OtpHash = otphash,
                FailedAttempts = 0,
                CreatedAt = _dateTimeProvider.UtcNow,
            };

            await _cacheService.SetAsync(RedisKeys.Registration(dto.Email), pending, TimeSpan.FromMinutes(10));

            await _emailService.SendOtpAsync(dto.Email, dto.FirstName, otp);

            return new RegisterResponse
            {
                Email = dto.Email,
                Message = "OTP has been sent to your email.",
                RequiresOtpVerification = true,
                Success = true
            };
        }
    }
}
