using CMS.Domain.Contents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Infrastructure.Persistence.Configurations;

public sealed class ContentConfiguration : IEntityTypeConfiguration<Content>
{
    public void Configure(EntityTypeBuilder<Content> b)
    {
        b.HasKey(c => c.Id);
        b.Property(c => c.Id)
            .HasConversion(id => id.Value, v => ContentId.From(v));
        b.Property(c => c.TenantId)
            .HasConversion(id => id.Value, v => Domain.Tenants.TenantId.From(v));

        b.Property(c => c.AuthorId)
            .HasConversion(id => id == null ? (Guid?)null : id.Value,
            value => value == null ? null : Domain.Identity.UserId.From(value.Value))
            .IsRequired(false);

        b.Property(c => c.Slug)
            .HasConversion(s => s.Value, v => Slug.Create(v))
            .HasMaxLength(200);

        b.Property(c => c.Status).IsRequired();

        b.Property(c => c.ContentType)
            .IsRequired()
            .HasDefaultValue(ContentType.Page);

        // PublishSchedule — owned (flat sütunlar)
        b.OwnsOne(c => c.Schedule, s =>
        {
            s.Property(x => x.PublishAt).HasColumnName("SchedulePublishAt");
            s.Property(x => x.UnpublishAt).HasColumnName("ScheduleUnpublishAt");
            s.Property(x => x.DurationMinutes).HasColumnName("ScheduleDurationMinutes");
            s.Property(x => x.TimeZoneId).HasColumnName("ScheduleTimeZoneId").HasMaxLength(50);
        });

        // ApprovalInfo — owned (flat sütunlar, nullable)
        b.OwnsOne(c => c.Approval, a =>
        {
            a.Property(x => x.SubmittedByUserId)
                .HasColumnName("ApprovalSubmittedByUserId")
                .HasConversion(
                    id => id.Value,
                    v => Domain.Identity.UserId.From(v));

            a.Property(x => x.SubmittedAt)
                .HasColumnName("ApprovalSubmittedAt");

            a.Property(x => x.Note)
                .HasColumnName("ApprovalNote")
                .HasMaxLength(1000);

            a.Property(x => x.ReviewedByUserId)
                .HasColumnName("ApprovalReviewedByUserId")
                .HasConversion(
                    id => id == null ? (Guid?)null : id.Value,
                    v => v == null ? null : Domain.Identity.UserId.From(v.Value))
                .IsRequired(false);

            a.Property(x => x.ReviewedAt)
                .HasColumnName("ApprovalReviewedAt")
                .IsRequired(false);

            a.Property(x => x.ReviewComment)
                .HasColumnName("ApprovalReviewComment")
                .HasMaxLength(2000)
                .IsRequired(false);

            a.Property(x => x.ApprovalStatus)
                .HasColumnName("ApprovalStatus")
                .HasMaxLength(20);
        });

        b.HasMany(c => c.Translations)
            .WithOne()
            .HasForeignKey(t => t.ContentId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasMany(c => c.Sections)
            .WithOne()
            .HasForeignKey(s => s.ContentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Soft delete global query filter — CmsDbContext'te tenant filter ile birlikte
        b.HasQueryFilter(c => c.DeletedAt == null);

        // Schedule watcher index'leri
        b.HasIndex(c => new { c.Status, c.Id })
            .HasFilter("[Status] = 1")
            .HasDatabaseName("IX_Contents_Scheduled");

        b.HasIndex(c => new { c.Status, c.Id })
            .HasFilter("[Status] = 2")
            .HasDatabaseName("IX_Contents_Published");

        // BUG FIX: Slug + TenantId birlikte index — cross-tenant slug izolasyonu
        b.HasIndex(c => new { c.TenantId, c.Slug })
            .HasDatabaseName("IX_Contents_TenantId_Slug");

        // ContentType filtresi için index (YENİ)
        b.HasIndex(c => new { c.TenantId, c.ContentType })
            .HasDatabaseName("IX_Contents_TenantId_ContentType");

        // AuthorId filtresi için index (YENİ)
        b.HasIndex(c => c.AuthorId)
            .HasDatabaseName("IX_Contents_AuthorId");

        b.ToTable("Contents");
    }
}
