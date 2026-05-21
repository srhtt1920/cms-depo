using CMS.Domain.Contents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Infrastructure.Persistence.Configurations;

public sealed class BlockTranslationConfiguration : IEntityTypeConfiguration<BlockTranslation>
{
    public void Configure(EntityTypeBuilder<BlockTranslation> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.BlockId)
            .HasConversion(id => id.Value, v => ContentBlockId.From(v));
        b.Property(x => x.LanguageCode).IsRequired().HasMaxLength(10);
        b.Property(x => x.Title).HasMaxLength(500);
        b.Property(x => x.AltText).HasMaxLength(500);
        b.Property(x => x.LinkText).HasMaxLength(500);
        b.Property(x => x.Body).HasColumnType("nvarchar(MAX)");

        b.HasIndex(x => new { x.BlockId, x.LanguageCode }).IsUnique();
        b.ToTable("BlockTranslations");
    }
}
