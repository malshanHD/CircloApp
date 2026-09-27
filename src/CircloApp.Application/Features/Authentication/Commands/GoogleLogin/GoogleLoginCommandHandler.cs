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

            if (googleUser is null || string.IsNullOrWhiteSpace(googleUser.GoogleSubject) || string.IsNullOrWhiteSpace(googleUser.Email))
            {
                throw new BadRequestException("Invalid Google credential.");
            }

            var user = await _userRepository.GetByExternalLoginAsync(GoogleProvider, googleUser.GoogleSubject, cancellationToken);

            if (user is null)
            {
                user = await _userRepository.GetByEmailAsync(googleUser.Email, cancellationToken);

                if (user is not null)
                {
                    if (!googleUser.IsAuthoritativeEmail)
                    {
                        throw new BadRequestException(
                            "An account already exists with this email. " +
                            "Sign in normally before linking Google.");
                    }

                    if (user.IsDeleted) throw new BadRequestException("This account is not available.");
                    user.EmailVerified = true;

                    // Repair rows created by the previous email-as-key bug only after authoritative email proof.
                    var legacyLogin = user.ExternalLogins.FirstOrDefault(x => x.Provider == GoogleProvider &&
                        string.Equals(x.ProviderSubject, googleUser.Email, StringComparison.OrdinalIgnoreCase));
                    if (legacyLogin is not null)
                    {
                        legacyLogin.ProviderSubject = googleUser.GoogleSubject;
                    }
                    else
                    {
                        var externalLogin = user.AddExternalLogin(GoogleProvider, googleUser.GoogleSubject);
                        if (externalLogin is not null)
                            await _userRepository.AddExternalLoginAsync(externalLogin, cancellationToken);
                    }
                }
                else
                {
                    user = CreateGoogleUser(googleUser);

                    user.AddExternalLogin(GoogleProvider, googleUser.GoogleSubject);

                    await _userRepository.AddAsync(user, cancellationToken);
                }
            }

            if (user.IsDeleted) throw new BadRequestException("This account is not available.");
            if (string.Equals(user.Email, googleUser.Email, StringComparison.OrdinalIgnoreCase))
                user.EmailVerified = true;

            var now = _dateProvider.UtcNow;
            var refreshToken = _refreshTokenGenerator.Generate();

            // Store the actual refresh token.
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = now.AddDays(7);

            // Save everything once:
            // new user, external login and refresh token.
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // For a new user, generate JWT after SaveChanges so its ID exists.
            var accessToken = _jwtTokenGenerator.GenerateToken(user);

            return new LoginResponse
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                AccessToken = accessToken.AccessToken,
                RefreshToken = refreshToken,
                ExpiresAt = accessToken.ExpiresAt
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
