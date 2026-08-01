using CircloApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CircloApp.Infrastructure.Configurations
{
    public class ExpensesConfiguration : IEntityTypeConfiguration<Expense>
    {
        public void Configure(EntityTypeBuilder<Expense> builder)
        {
            builder.ToTable("Expenses");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Description)
                   .HasMaxLength(200)
                   .IsRequired();

            builder.Property(x => x.Amount)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(x => x.ExpenseDate)
                   .IsRequired();

            builder.HasOne(x => x.Event)
                   .WithMany(x => x.Expenses)
                   .HasForeignKey(x => x.EventId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PaidByUser)
                   .WithMany(x => x.Expenses)
                   .HasForeignKey(x => x.PaidByUserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.EventId);

            builder.HasIndex(x => x.PaidByUserId);
        }
    }
}
