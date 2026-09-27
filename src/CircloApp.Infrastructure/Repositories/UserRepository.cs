using CircloApp.Application.Features.Authentication.DTOs;
using CircloApp.Application.Interfaces;
using CircloApp.Domain.Entities;
using CircloApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CircloApp.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        public Task<bool> ExistsByIdAsync(Guid userId, CancellationToken cancellationToken) =>
            _context.Users.AnyAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);
        private readonly ApplicationDbContext _context;
        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken) =>
            _context.Users.AsTracking().Include(u => u.ExternalLogins)
                .SingleOrDefaultAsync(u => u.Email == email, cancellationToken);

        public async Task AddExternalLoginAsync(UserExternalLogin externalLogin, CancellationToken cancellationToken)
        {
            // A preassigned GUID discovered through a navigation can otherwise be marked Modified.
            await _context.UserExternalLogins.AddAsync(externalLogin, cancellationToken);
        }

        public async Task AddAsync(User user, CancellationToken cancellationToken)
        {
            await _context.AddAsync(user, cancellationToken);
        }

        public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken)
        {
            return await _context.Users.AnyAsync(u => u.Email == email, cancellationToken);
        }

        public async Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken)
        {
            return await _context.Users.AnyAsync(u => u.Username == username, cancellationToken);
        }

        public async Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail)
        {
            return await _context.Users.AsTracking().FirstOrDefaultAsync(u => u.Username == usernameOrEmail || u.Email == usernameOrEmail);
        }

        public async Task<List<GetUserResponse>> SearchUserByUsername(string username, CancellationToken cancellationToken)
        {
            return await _context.Users.Where(u => u.Username
                                                .Contains(username)).Take(10)
                                                .Select(user => new GetUserResponse
                                                {
                                                    Username = user.Username
                                                }).ToListAsync(cancellationToken);
        }

        public async Task<User?> GetByExternalLoginAsync(string provider, string providerSubject, CancellationToken cancellationToken = default)
        {
            return await _context.Users.AsTracking().Include(u => u.ExternalLogins)
                .SingleOrDefaultAsync(u => u.ExternalLogins.Any(x => x.Provider == provider && x.ProviderSubject == providerSubject), cancellationToken);
        }
    }
}
