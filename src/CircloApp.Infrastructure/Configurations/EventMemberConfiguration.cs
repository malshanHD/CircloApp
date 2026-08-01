using CircloApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CircloApp.Infrastructure.Configurations
{
    public class EventMemberConfiguration : IEntityTypeConfiguration<EventMember>
    {
        public void Configure(EntityTypeBuilder<EventMember> builder)
        {
            builder.ToTable("EventMembers");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Role)
                   .HasMaxLength(30)
                   .IsRequired();

            builder.Property(x => x.JoinedAt)
                   .IsRequired();

            builder.HasOne(x => x.Event)
                   .WithMany(x => x.Members)
                   .HasForeignKey(x => x.EventId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.User)
                   .WithMany(x => x.EventMemberships)
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.EventId, x.UserId })
                   .IsUnique();
        }
    }
}
