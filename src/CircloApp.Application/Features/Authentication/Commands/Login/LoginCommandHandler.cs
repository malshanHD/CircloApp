using CircloApp.Application.Exceptions;
using CircloApp.Application.Features.Authentication.DTOs;
using CircloApp.Application.Interfaces;
using MediatR;

namespace CircloApp.Application.Features.Authentication.Commands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRefreshTokenGenerator _refreshTokenGenerator;

        public LoginCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, IDateTimeProvider dateTimeProvider, 
                                   IJwtTokenGenerator jwtTokenGenerator, IUnitOfWork unitOfWork, IRefreshTokenGenerator refreshTokenGenerator)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _dateTimeProvider = dateTimeProvider;
            _jwtTokenGenerator = jwtTokenGenerator;
            _unitOfWork = unitOfWork;
            _refreshTokenGenerator = refreshTokenGenerator;
        }

        public async Task<LoginResponse> Handle(LoginCommand command, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByUsernameOrEmailAsync(command.Request.UsernameOrEmail);
            if (user is null)
                throw new BadRequestException("Invalid username/email or password.");

            var passwordValid = _passwordHasher.VerifyPassword(command.Request.Password, user.PasswordHash);
            if (!passwordValid)
                throw new BadRequestException("Invalid username/email or password.");

            var accessToken = _jwtTokenGenerator.GenerateToken(user);

            var refreshToken = _refreshTokenGenerator.Generate();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = _dateTimeProvider.UtcNow.AddDays(7);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new LoginResponse
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = _dateTimeProvider.UtcNow.AddMinutes(20)
            };
        }
    }
}
