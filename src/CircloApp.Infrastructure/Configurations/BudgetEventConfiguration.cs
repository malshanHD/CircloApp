using CircloApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CircloApp.Infrastructure.Configurations
{
    public class BudgetEventConfiguration : IEntityTypeConfiguration<BudgetEvent>
    {
        public void Configure(EntityTypeBuilder<BudgetEvent> builder)
        {
            builder.ToTable("BudgetEvents");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(x => x.Description)
                   .HasMaxLength(500);

            builder.HasOne(x => x.CreatedByUser)
                   .WithMany(x => x.CreatedEvents)
                   .HasForeignKey(x => x.CreatedByUserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
