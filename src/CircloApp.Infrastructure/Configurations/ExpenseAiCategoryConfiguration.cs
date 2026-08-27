using CircloApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CircloApp.Infrastructure.Configurations
{
    public class ExpenseAiCategoryConfiguration : IEntityTypeConfiguration<ExpenseAiCategory>
    {
        public void Configure(EntityTypeBuilder<ExpenseAiCategory> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Category)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.Model)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasIndex(x => x.ExpenseId)
                .IsUnique();

            builder.HasOne(x => x.Expense)
                .WithOne()
                .HasForeignKey<ExpenseAiCategory>(x => x.ExpenseId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
