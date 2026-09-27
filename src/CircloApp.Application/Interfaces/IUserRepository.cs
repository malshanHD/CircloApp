using CircloApp.Application.Features.Authentication.DTOs;
using CircloApp.Domain.Entities;

namespace CircloApp.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);
        Task AddExternalLoginAsync(UserExternalLogin externalLogin, CancellationToken cancellationToken);
        Task<bool> ExistsByIdAsync(Guid userId, CancellationToken cancellationToken);
        Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken);
        Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken);
        Task AddAsync(User user, CancellationToken cancellationToken);
        Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail);
        Task<List<GetUserResponse>> SearchUserByUsername(string username, CancellationToken cancellationToken);
        Task<User?> GetByExternalLoginAsync(string provider, string providerSubject, CancellationToken cancellationToken = default);
    }
}
