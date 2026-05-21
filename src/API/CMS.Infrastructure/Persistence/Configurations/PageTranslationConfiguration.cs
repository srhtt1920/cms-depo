using CMS.Domain.Pages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Infrastructure.Persistence.Configurations;

public sealed class PageTranslationConfiguration : IEntityTypeConfiguration<PageTranslation>
{
    public void Configure(EntityTypeBuilder<PageTranslation> b)
    {
        b.HasKey(t => t.Id);

        b.Property(t => t.PageId)
            .HasConversion(id => id.Value, v => PageId.From(v));

        b.Property(t => t.LanguageCode).HasMaxLength(10).IsRequired();
        b.Property(t => t.Title).HasMaxLength(500).IsRequired();
        b.Property(t => t.LinkName).HasMaxLength(200).IsRequired();
        b.Property(t => t.Slug).HasMaxLength(500).IsRequired();
        b.Property(t => t.MetaTitle).HasMaxLength(200).IsRequired(false);
        b.Property(t => t.MetaDescription).HasMaxLength(500).IsRequired(false);
        b.Property(t => t.MetaKeywords).HasMaxLength(300).IsRequired(false);

        // Bir sayfa için her dilde yalnızca bir çeviri
        b.HasIndex(t => new { t.PageId, t.LanguageCode })
            .IsUnique()
            .HasDatabaseName("IX_PageTranslations_PageId_LangCode");

        // Slug benzersizliği tenant+dil bazında — Repository'de de kontrol edilir
        b.HasIndex(t => new { t.Slug, t.LanguageCode })
            .HasDatabaseName("IX_PageTranslations_Slug_LangCode");

        b.ToTable("PageTranslations");
    }
}

