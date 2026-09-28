using CircloApp.Application.Interfaces;
using CircloApp.Domain.Entities;
using CircloApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CircloApp.Infrastructure.Repositories
{
    public class PersonalExpenseCategoryRepository : IPersonalExpenseCategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public PersonalExpenseCategoryRepository(ApplicationDbContext applicationDbContext)
        {
            _context = applicationDbContext;
        }

        public async Task<IReadOnlyList<PersonalExpenseCategory>> GetAvailableForUserAsync(Guid userId, CancellationToken cancellationTokenq = default)
        {
            return await _context.PersonalExpenseCategories
                .AsNoTracking()
                .Where(cat => cat.IsActive && (cat.IsSystem || cat.UserId == userId))
                .OrderBy(cat => cat.Name == "Uncategorized" ? 1 : 0)
                .ThenBy(cat => cat.Name)
                .ToListAsync(cancellationTokenq);
        }

        public async Task<Guid?> GetUncategorizedIdAsync(CancellationToken cancellationToken = default)
        {
            return await _context.PersonalExpenseCategories
                .AsNoTracking()
                .Where(category =>
                    category.IsSystem &&
                    category.IsActive &&
                    category.Name == "Uncategorized")
                .Select(category => (Guid?)category.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<bool> IsAvailableForUserAsync(Guid categoryId, Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.PersonalExpenseCategories
                .AsNoTracking()
                .AnyAsync(
                    category =>
                        category.Id == categoryId &&
                        category.IsActive &&
                        (
                            category.IsSystem ||
                            category.UserId == userId
                        ),
                    cancellationToken);
        }
    }
}
