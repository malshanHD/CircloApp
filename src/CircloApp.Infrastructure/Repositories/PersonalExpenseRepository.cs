using CircloApp.Application.Interfaces;
using CircloApp.Domain.Entities;
using CircloApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using CircloApp.Application.Features.PersonalExpenses.DTOs;

namespace CircloApp.Infrastructure.Repositories
{
    public class PersonalExpenseRepository : IPersonalExpenseRepository
    {
        private readonly ApplicationDbContext _context;

        public PersonalExpenseRepository(ApplicationDbContext applicationDbContext)
        {
            _context = applicationDbContext;
        }

        public async Task AddAsync(PersonalExpense personalExpense, CancellationToken cancellationToken = default)
        {
            await _context.PersonalExpenses.AddAsync(personalExpense, cancellationToken);
        }

        public Task<PersonalExpense?> GetAsync(Guid userId, Guid id, bool tracking, CancellationToken ct)
        {
            var query = _context.PersonalExpenses.Where(e => e.UserId == userId && e.Id == id && !e.IsDeleted);
            return (tracking ? query : query.AsNoTracking()).Include(e => e.Category).SingleOrDefaultAsync(ct);
        }
        public async Task<(IReadOnlyList<PersonalExpense> Items, int Count)> ListAsync(Guid userId,
            ExpenseFilter f, DateOnly? from, DateOnly? to, CancellationToken ct)
        {
            var query = _context.PersonalExpenses.AsNoTracking().Where(e => e.UserId == userId && !e.IsDeleted);
            if (from.HasValue) query = query.Where(e => e.ExpenseDate >= from.Value);
            if (to.HasValue) query = query.Where(e => e.ExpenseDate < to.Value);
            if (f.CategoryId.HasValue) query = query.Where(e => e.CategoryId == f.CategoryId);
            if (!string.IsNullOrWhiteSpace(f.Search)) query = query.Where(e => e.Description.Contains(f.Search.Trim()) || (e.Note != null && e.Note.Contains(f.Search.Trim())));
            if (f.MinAmount.HasValue) query = query.Where(e => e.Amount >= f.MinAmount);
            if (f.MaxAmount.HasValue) query = query.Where(e => e.Amount <= f.MaxAmount);
            if (!string.IsNullOrWhiteSpace(f.PaymentMethod)) query = query.Where(e => e.PaymentMethod == f.PaymentMethod.Trim());
            var count = await query.CountAsync(ct);
            var asc = f.SortDirection == "asc";
            var sorted = f.SortBy switch
            {
                "amount" => asc ? query.OrderBy(e => e.Amount) : query.OrderByDescending(e => e.Amount),
                "description" => asc ? query.OrderBy(e => e.Description) : query.OrderByDescending(e => e.Description),
                "category" => asc ? query.OrderBy(e => e.Category!.Name) : query.OrderByDescending(e => e.Category!.Name),
                "createdat" => asc ? query.OrderBy(e => e.CreatedAt) : query.OrderByDescending(e => e.CreatedAt),
                _ => asc ? query.OrderBy(e => e.ExpenseDate) : query.OrderByDescending(e => e.ExpenseDate)
            };
            var items = await sorted.ThenByDescending(e => e.CreatedAt).ThenByDescending(e => e.Id)
                .Skip((f.Page - 1) * f.PageSize).Take(f.PageSize).Include(e => e.Category).ToListAsync(ct);
            return (items, count);
        }
    }
}
