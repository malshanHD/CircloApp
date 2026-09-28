using CircloApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CircloApp.Infrastructure.Configurations
{
    public class PersonalExpenseConfiguration : IEntityTypeConfiguration<PersonalExpense>
    {
        public void Configure(EntityTypeBuilder<PersonalExpense> builder)
        {
            builder.Property(x => x.Description).HasMaxLength(250).IsRequired();

            builder.Property(x => x.Amount).HasPrecision(18, 2);

            builder.Property(x => x.ExpenseDate).HasColumnType("date");

            builder.Property(x => x.PaymentMethod).HasMaxLength(50);

            builder.Property(x => x.Note).HasMaxLength(500);

            builder.HasIndex(x => new
            {
                x.UserId,
                x.ExpenseDate
            });

            builder.HasIndex(x => new
            {
                x.UserId,
                x.CategoryId,
                x.ExpenseDate
            });
        }
    }
}
