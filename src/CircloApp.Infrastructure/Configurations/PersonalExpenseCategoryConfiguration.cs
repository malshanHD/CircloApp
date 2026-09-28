using CircloApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CircloApp.Infrastructure.Configurations
{
    public class PersonalExpenseCategoryConfiguration : IEntityTypeConfiguration<PersonalExpenseCategory>
    {
        public void Configure(EntityTypeBuilder<PersonalExpenseCategory> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Icon)
                .HasMaxLength(50);

            builder.Property(x => x.Color)
                .HasMaxLength(20);

            builder.HasOne(x => x.User)
                .WithMany(x => x.PersonalExpenseCategories)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new
            {
                x.UserId,
                x.Name
            })
            .IsUnique();

            var seedDate = new DateTime(
                2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            builder.HasData(
                CreateCategory(
                    "11111111-1111-1111-1111-111111111111",
                    "Food",
                    "utensils",
                    "#F59E0B",
                    seedDate),

                CreateCategory(
                    "22222222-2222-2222-2222-222222222222",
                    "Transport",
                    "car",
                    "#3B82F6",
                    seedDate),

                CreateCategory(
                    "33333333-3333-3333-3333-333333333333",
                    "Shopping",
                    "shopping-bag",
                    "#EC4899",
                    seedDate),

                CreateCategory(
                    "44444444-4444-4444-4444-444444444444",
                    "Bills",
                    "receipt",
                    "#8B5CF6",
                    seedDate),

                CreateCategory(
                    "55555555-5555-5555-5555-555555555555",
                    "Health",
                    "heart",
                    "#EF4444",
                    seedDate),

                CreateCategory(
                    "66666666-6666-6666-6666-666666666666",
                    "Entertainment",
                    "film",
                    "#14B8A6",
                    seedDate),

                CreateCategory(
                    "77777777-7777-7777-7777-777777777777",
                    "Education",
                    "book",
                    "#6366F1",
                    seedDate),

                CreateCategory(
                    "88888888-8888-8888-8888-888888888888",
                    "Travel",
                    "plane",
                    "#06B6D4",
                    seedDate),

                CreateCategory(
                    "99999999-9999-9999-9999-999999999999",
                    "Other",
                    "circle",
                    "#64748B",
                    seedDate),

                CreateCategory(
                    "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
                    "Uncategorized",
                    "help-circle",
                    "#94A3B8",
                    seedDate)
            );
        }

        private static PersonalExpenseCategory CreateCategory(string id, string name, string icon, string color, DateTime createdAt)
        {
            return new PersonalExpenseCategory
            {
                Id = Guid.Parse(id),
                UserId = null,
                Name = name,
                Icon = icon,
                Color = color,
                IsSystem = true,
                IsActive = true,
                CreatedAt = createdAt,
                UpdatedAt = createdAt,
            };
        }
    }
}
