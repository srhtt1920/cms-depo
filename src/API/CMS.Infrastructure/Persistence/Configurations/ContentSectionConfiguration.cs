using CMS.Domain.Contents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace CMS.Infrastructure.Persistence.Configurations;

public sealed class ContentSectionConfiguration : IEntityTypeConfiguration<ContentSection>
{
    public void Configure(EntityTypeBuilder<ContentSection> b)
    {
        b.HasKey(s => s.Id);
        b.Property(s => s.Id)
            .HasConversion(id => id.Value, v => ContentSectionId.From(v));
        b.Property(s => s.ContentId)
            .HasConversion(id => id.Value, v => ContentId.From(v));
        b.Property(s => s.ParentSectionId)
            .HasConversion(id => id == null ? (Guid?)null : id.Value,
                           v  => v.HasValue ? ContentSectionId.From(v.Value) : null);

        b.Property(s => s.Name).IsRequired().HasMaxLength(200);
        b.Property(s => s.CssClass).HasMaxLength(500);

        // SectionAnimationSettings owned entity (JSON column)
        b.OwnsOne(s => s.Animation, a =>
        {
            a.Property(x => x.Type).HasColumnName("AnimationType").HasMaxLength(50);
            a.Property(x => x.Duration).HasColumnName("AnimationDuration");
            a.Property(x => x.Delay).HasColumnName("AnimationDelay");
            a.Property(x => x.Easing).HasColumnName("AnimationEasing").HasMaxLength(50);
        });

        b.HasMany(s => s.Blocks)
            .WithOne()
            .HasForeignKey(b => b.SectionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Self-referencing tree
        b.HasOne<ContentSection>()
            .WithMany()
            .HasForeignKey(s => s.ParentSectionId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(s => new { s.ContentId, s.Order });
        b.ToTable("ContentSections");
    }
}
