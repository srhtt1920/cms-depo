using CMS.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("Id")
            .HasConversion(
                id => id.Value,
                value => UserId.From(value))
            .IsRequired();

        builder.Property(u => u.Email)
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(u => u.PasswordHash)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(u => u.PermissionVersion)
            .HasDefaultValue(1)
            .IsRequired();

        builder.Property(u => u.CreatedAt).IsRequired();

        builder.Property(u => u.UpdatedAt);

        builder.Property(u => u.IsSuperAdmin)
            .HasDefaultValue(false);

        builder.Property(u => u.SuperAdminGrantedAt)
            .IsRequired(false);

        builder.Navigation(u => u.TenantRoles)
            .HasField("_tenantRoles")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(u => u.TenantRoles)
            .WithOne()
            .HasForeignKey(utr => utr.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Ignore(u => u.DomainEvents);
    }
}
