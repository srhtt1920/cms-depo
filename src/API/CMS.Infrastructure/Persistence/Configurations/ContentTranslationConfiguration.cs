using CMS.Domain.Contents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Infrastructure.Persistence.Configurations;

public sealed class ContentTranslationConfiguration : IEntityTypeConfiguration<ContentTranslation>
{
    public void Configure(EntityTypeBuilder<ContentTranslation> builder)
    {
        builder.ToTable("ContentTranslations");

        builder.HasKey(ct => ct.Id);

        builder.Property(ct => ct.ContentId)
            .HasColumnName("ContentId")
            .HasConversion(
                id => id.Value,
                value => ContentId.From(value))
            .IsRequired();

        builder.Property(ct => ct.LanguageCode)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(ct => ct.Title)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(t => t.Body)
             .HasColumnType("nvarchar(MAX)");

        builder.Property(ct => ct.MetaTitle)
            .HasMaxLength(200);

        builder.Property(ct => ct.MetaDescription)
            .HasMaxLength(500);

        builder.Property(ct => ct.IsPublished)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(ct => ct.CreatedAt).IsRequired();
        builder.Property(ct => ct.UpdatedAt);

        // Aynı content'in aynı dilden sadece bir çevirisi olabilir
        builder.HasIndex(ct => new { ct.ContentId, ct.LanguageCode })
            .IsUnique();

        // Navigation — EF Core için
        builder.HasOne<Content>()
            .WithMany(c => c.Translations)
            .HasForeignKey(ct => ct.ContentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
