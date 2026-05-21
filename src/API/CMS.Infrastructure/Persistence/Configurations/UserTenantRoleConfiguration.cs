using CMS.Domain.Identity;
using CMS.Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Infrastructure.Persistence.Configurations;

public sealed class UserTenantRoleConfiguration : IEntityTypeConfiguration<UserTenantRole>
{
    public void Configure(EntityTypeBuilder<UserTenantRole> builder)
    {
        builder.ToTable("UserTenantRoles");

        builder.HasKey(utr => utr.Id);

        builder.Property(utr => utr.UserId)
            .HasConversion(
                id => id.Value,
                value => UserId.From(value))
            .IsRequired();

        builder.Property(utr => utr.TenantId)
            .HasConversion(
                id => id.Value,
                value => TenantId.From(value))
            .IsRequired();

        builder.Property(utr => utr.RoleId)
            .HasConversion(
                id => id.Value,
                value => RoleId.From(value))
            .IsRequired();

        builder.Property(utr => utr.AssignedAt).IsRequired();

        // Aynı user → tenant → role ataması bir kez olabilir
        builder.HasIndex(utr => new { utr.UserId, utr.TenantId, utr.RoleId })
            .IsUnique();
    }
}
