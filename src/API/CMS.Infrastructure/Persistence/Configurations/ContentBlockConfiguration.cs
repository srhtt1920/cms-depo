using CMS.Domain.Contents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Infrastructure.Persistence.Configurations;

public sealed class ContentBlockConfiguration : IEntityTypeConfiguration<ContentBlock>
{
    public void Configure(EntityTypeBuilder<ContentBlock> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Id)
            .HasConversion(id => id.Value, v => ContentBlockId.From(v));
        b.Property(x => x.SectionId)
            .HasConversion(id => id.Value, v => ContentSectionId.From(v));

        b.Property(x => x.BlockType).IsRequired();
        b.Property(x => x.Settings).HasColumnType("nvarchar(MAX)");

        b.HasMany(x => x.Translations)
            .WithOne()
            .HasForeignKey(t => t.BlockId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => new { x.SectionId, x.Order });
        b.ToTable("ContentBlocks");
    }
}
