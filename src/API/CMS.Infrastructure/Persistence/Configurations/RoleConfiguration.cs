using CMS.Domain.Identity;
using CMS.Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Infrastructure.Persistence.Configurations;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("Id")
            .HasConversion(
                id => id.Value,
                value => RoleId.From(value))
            .IsRequired();

        builder.Property(r => r.TenantId)
            .HasColumnName("TenantId")
            .HasConversion(
                id => id.Value,
                value => TenantId.From(value))
            .IsRequired();

        builder.Property(r => r.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(r => r.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(r => r.CreatedAt).IsRequired();

        // Role → Permission many-to-many
        builder.HasMany(r => r.Permissions)
            .WithMany()
            .UsingEntity<RolePermissionJoin>(
                j => j
                    .HasOne<Permission>()
                    .WithMany()
                    .HasForeignKey(x => x.PermissionId),

                j => j
                    .HasOne<Role>()
                    .WithMany()
                    .HasForeignKey(x => x.RoleId),

                j =>
                {
                    j.ToTable("RolePermissions");

                    j.HasKey(x => new { x.RoleId, x.PermissionId });

                    j.Property(x => x.RoleId)
                        .HasConversion(
                            id => id.Value,
                            value => RoleId.From(value));

                    j.Property(x => x.PermissionId)
                        .HasConversion(
                            id => id.Value,
                            value => PermissionId.From(value));
                });

        builder.HasIndex(r => new { r.TenantId, r.Name })
            .IsUnique();

        builder.Ignore(r => r.DomainEvents);
    }
}

// EF Core many-to-many join entity
internal sealed class RolePermissionJoin
{
    public RoleId RoleId { get; set; } = default!;
    public PermissionId PermissionId { get; set; } = default!;
}
