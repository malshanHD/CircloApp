using CircloApp.Application.Common.Interfaces;
using CircloApp.Application.Exceptions;
using CircloApp.Application.Features.Authentication.DTOs;
using CircloApp.Application.Interfaces;
using CircloApp.Domain.Entities;
using MediatR;
using System.Security.Cryptography;

namespace CircloApp.Application.Features.Authentication.Commands.GoogleLogin
{
    public class GoogleLoginCommandHandler : IRequestHandler<GoogleLoginCommand, LoginResponse>
    {
        private const string GoogleProvider = "Google";
        private readonly IUserRepository _userRepository;
        private readonly IGoogleTokenValidator _googleTokenValidator;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IDateTimeProvider _dateProvider;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IRefreshTokenGenerator _refreshTokenGenerator;
        private readonly IUnitOfWork _unitOfWork;

        public GoogleLoginCommandHandler(IUserRepository userRepository, IGoogleTokenValidator googleToken, IPasswordHasher passwordHasher,
                                         IDateTimeProvider dateTimeProvider, IJwtTokenGenerator jwtTokenGenerator, IRefreshTokenGenerator refreshTokenGenerator, 
                                         IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _googleTokenValidator = googleToken;
            _passwordHasher = passwordHasher;
            _dateProvider = dateTimeProvider;
            _jwtTokenGenerator = jwtTokenGenerator;
            _refreshTokenGenerator = refreshTokenGenerator;
            _unitOfWork = unitOfWork;
        }

        public async Task<LoginResponse> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
        {
            var googleUser = await _googleTokenValidator.ValidateAsync(request.Request.IdToken, cancellationToken);

            if(googleUser is null)
            {
                throw new BadRequestException("Invalid username/email or password.");
            }

            var user = await _userRepository.GetByExternalLoginAsync(GoogleProvider, googleUser.GoogleSubject, cancellationToken);

            if (user is null)
            {
                user = await _userRepository.GetByUsernameOrEmailAsync(googleUser.Email);
                if (user is not null)
                {
                    if (!googleUser.IsAuthoritativeEmail)
                    {
                        throw new BadRequestException("An account already exists with this email." + "Sign in normally before linking Google");
                    }
                    user.EmailVerified = true;
                    user.AddExternalLogin(GoogleProvider, googleUser.Email);
                }

                else
                {
                    user = CreateGoogleUser(googleUser);
                    user.AddExternalLogin(GoogleProvider, googleUser.GoogleSubject);
                    await _userRepository.AddAsync(user, cancellationToken);
                }
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            var now = _dateProvider.UtcNow;
            var accessToken = _jwtTokenGenerator.GenerateToken(user);
            var refreshToken = _refreshTokenGenerator.Generate();

            user.RefreshToken = accessToken;
            user.RefreshTokenExpiryTime = now.AddDays(7);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new LoginResponse
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = now.AddMinutes(60)
            };
        }

        private User CreateGoogleUser(GoogleUserInfo googleUserInfo)
        {
            var randomPassword = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            return new User
            {
                FirstName = googleUserInfo.GivenName ?? googleUserInfo.Name ?? "Circlo",
                LastName = googleUserInfo.FamilyName ?? string.Empty,
                Email = googleUserInfo.Email,
                Username = $"google_{Guid.NewGuid():N}"[..19],
                ContactNumber = string.Empty,
                PasswordHash = _passwordHasher.HashPassword(randomPassword),
                EmailVerified = true
            };
        }
    }
}
