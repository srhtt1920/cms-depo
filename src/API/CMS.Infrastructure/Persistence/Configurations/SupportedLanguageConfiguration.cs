using CMS.Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Infrastructure.Persistence.Configurations;

public sealed class SupportedLanguageConfiguration : IEntityTypeConfiguration<SupportedLanguage>
{
    public void Configure(EntityTypeBuilder<SupportedLanguage> builder)
    {
        builder.ToTable("SupportedLanguages");

        builder.HasKey(sl => sl.Id);

        builder.Property(sl => sl.TenantId)
            .HasColumnName("TenantId")
            .HasConversion(
                id => id.Value,
                value => TenantId.From(value))
            .IsRequired();

        builder.Property(sl => sl.LanguageCode)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(sl => sl.IsDefault)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(sl => sl.IsFallback)
            .HasDefaultValue(false)
            .IsRequired();

        // Tenant başına aynı dil bir kez olabilir
        builder.HasIndex(sl => new { sl.TenantId, sl.LanguageCode })
            .IsUnique();
    }
}
