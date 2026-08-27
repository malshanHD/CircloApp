using CircloApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CircloApp.Infrastructure.Configurations
{
    public class EventAiAnalysisConfiguration : IEntityTypeConfiguration<EventAiAnalysis>
    {
        public void Configure(EntityTypeBuilder<EventAiAnalysis> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Summary)
                .IsRequired();

            builder.Property(x => x.DataHash)
                .HasMaxLength(64)
                .IsRequired();

            builder.Property(x => x.Model)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .IsRequired();

            builder.HasIndex(x => x.EventId)
                .IsUnique();

            builder.HasOne(x => x.Event)
                .WithOne()
                .HasForeignKey<EventAiAnalysis>(x => x.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
