using CMS.Domain.Contents;
using CMS.Domain.Pages;
using CMS.Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.Infrastructure.Persistence.Configurations;

public sealed class PageConfiguration : IEntityTypeConfiguration<Page>
{
    public void Configure(EntityTypeBuilder<Page> b)
    {
        b.HasKey(p => p.Id);

        b.Property(p => p.Id)
            .HasConversion(id => id.Value, v => PageId.From(v));

        b.Property(p => p.TenantId)
            .HasConversion(id => id.Value, v => TenantId.From(v));

        b.Property(p => p.ParentId)
            .HasConversion(
                id => id == null ? (Guid?)null : id.Value,
                v => v == null ? null : PageId.From(v.Value))
            .IsRequired(false);

        b.Property(p => p.LinkedContentId)
            .HasConversion(
                id => id == null ? (Guid?)null : id.Value,
                v => v == null ? null : ContentId.From(v.Value))
            .IsRequired(false);

        b.Property(p => p.PageType).IsRequired();
        b.Property(p => p.ContentType).IsRequired(false);
        b.Property(p => p.Order).HasDefaultValue(0);
        b.Property(p => p.IsActive).HasDefaultValue(true);
        b.Property(p => p.IsVisible).HasDefaultValue(true);
        b.Property(p => p.Icon).HasMaxLength(200).IsRequired(false);
        b.Property(p => p.ExternalUrl).HasMaxLength(2000).IsRequired(false);

        // Çeviriler
        b.HasMany(p => p.Translations)
            .WithOne()
            .HasForeignKey("PageId")
            .OnDelete(DeleteBehavior.Cascade);

        // Self-referencing FK (tree yapısı)
        // Cascade DELETE yapılmaz — üst silinince altlar yetim kalır, sonra ayrı işlenir.
        b.HasOne<Page>()
            .WithMany()
            .HasForeignKey(p => p.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Soft delete global query filter
        b.HasQueryFilter(p => p.DeletedAt == null);

        // ── Index'ler ─────────────────────────────────────────────────────────

        // Tree navigation: tenant → parent → sıra
        b.HasIndex(p => new { p.TenantId, p.ParentId, p.Order })
            .HasDatabaseName("IX_Pages_TenantId_ParentId_Order");

        // Tip bazlı listeleme
        b.HasIndex(p => new { p.TenantId, p.PageType })
            .HasDatabaseName("IX_Pages_TenantId_PageType");

        // İçerik bağlantısı
        b.HasIndex(p => p.LinkedContentId)
            .HasDatabaseName("IX_Pages_LinkedContentId");

        b.ToTable("Pages");
    }
}
