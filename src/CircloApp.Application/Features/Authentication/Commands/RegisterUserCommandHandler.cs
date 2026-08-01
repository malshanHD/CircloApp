using CircloApp.Application.Exceptions;
using CircloApp.Application.Features.Authentication.DTOs;
using CircloApp.Application.Interfaces;
using CircloApp.Domain.Entities;
using MediatR;

namespace CircloApp.Application.Features.Authentication.Commands
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterUserResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUnitOfWork _unitOfWork;

        public RegisterUserCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _unitOfWork = unitOfWork;
        }

        public async Task<RegisterUserResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
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

            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Username = dto.Username,
                ContactNumber = dto.ContactNumber,
                PasswordHash = _passwordHasher.HashPassword(dto.Password)
            };

            await _userRepository.AddAsync(user, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new RegisterUserResponse
            {
                UserId = user.Id,
                Message = "User registered successfully."
            };
        }
    }
}
