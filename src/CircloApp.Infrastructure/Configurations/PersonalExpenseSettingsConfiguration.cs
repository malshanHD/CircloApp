using CircloApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CircloApp.Infrastructure.Configurations
{
    public class PersonalExpenseSettingsConfiguration : IEntityTypeConfiguration<PersonalExpenseSettings>
    {
        public void Configure(EntityTypeBuilder<PersonalExpenseSettings> builder)
        {
            builder.HasKey(x => x.UserId);
            builder.HasOne(x => x.user).WithOne().HasForeignKey<PersonalExpenseSettings>(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            builder.Property(x => x.DefaultMonthlyLimit).HasPrecision(18, 2);
            builder.Property(x => x.WarningPercentage).HasPrecision(5, 2);
            builder.Property(x => x.CurrencyCode).HasMaxLength(3);
        }
    }
}
