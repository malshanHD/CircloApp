using CircloApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CircloApp.Infrastructure.Configurations
{
    public class MonthlyExpenseBudgetConfiguration : IEntityTypeConfiguration<MonthlyExpenseBudget>
    {
        public void Configure(EntityTypeBuilder<MonthlyExpenseBudget> builder)
        {
            builder.HasIndex(x => new
            {
                x.UserId,
                x.Year,
                x.Month
            }).IsUnique();

            builder.Property(x => x.LimitAmount).HasPrecision(18, 2);

            builder.ToTable(table =>
            {
                table.HasCheckConstraint(
                    "CK_MonthlyExpenseBudget_Month",
                    "[Month] >= 1 AND [Month] <= 12");

                table.HasCheckConstraint(
                    "CK_MonthlyExpenseBudget_LimitAmount",
                    "[LimitAmount] >= 0");
            });
        }
    }
}
