using CMS.Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Infrastructure.Persistence.Configurations;

public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasConversion(id => id.Value, v => TenantId.From(v));

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.DefaultLanguageCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(t => t.Type)
            .IsRequired()
            .HasDefaultValue(TenantType.Business);

        builder.Property(t => t.IsActive).IsRequired();
        builder.Property(t => t.IsMaintenanceMode).IsRequired();
        builder.Property(t => t.CreatedAt).IsRequired();
        builder.Property(t => t.UpdatedAt);

        builder.HasIndex(t => t.Name).IsUnique();
        builder.HasIndex(t => t.Type); 

        builder.HasMany(t => t.SupportedLanguages)
            .WithOne()
            .HasForeignKey(sl => sl.TenantId);
    }
}
