using CMS.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Infrastructure.Persistence.Configurations;

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("Id")
            .HasConversion(
                id => id.Value,
                value => PermissionId.From(value))
            .IsRequired();

        builder.Property(p => p.Key)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.GroupKey)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.DisplayName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.ParentId)
            .HasColumnName("ParentId")
            .HasConversion(
                id => id == null ? (Guid?)null : id.Value,
                value => value == null ? null : PermissionId.From(value.Value));

        // Self-referencing — parent permission
        builder.HasOne<Permission>()
            .WithMany()
            .HasForeignKey(p => p.ParentId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.Key).IsUnique();
        builder.HasIndex(p => p.GroupKey);
    }
}

//public sealed class PermissionDataConfiguration : IEntityTypeConfiguration<Permission>
//{
//    public void Configure(EntityTypeBuilder<Permission> builder)
//    {
//        builder.HasData(PermissionSeed.GetSeedData());
//    }
//}
