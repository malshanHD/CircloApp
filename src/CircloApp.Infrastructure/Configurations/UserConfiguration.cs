using CircloApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CircloApp.Infrastructure.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.FirstName)
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(x => x.LastName)
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(x => x.Email)
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(x => x.Username)
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(x => x.ContactNumber)
                   .HasMaxLength(15);

            builder.Property(x => x.PasswordHash)
                   .IsRequired();

            builder.HasIndex(x => x.Email)
                   .IsUnique();

            builder.HasIndex(x => x.Username)
                   .IsUnique();
        }
    }
}
