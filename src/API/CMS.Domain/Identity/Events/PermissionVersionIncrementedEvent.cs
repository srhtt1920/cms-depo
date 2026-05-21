using CMS.Domain.Common;
using CMS.Domain.Tenants;

namespace CMS.Domain.Identity.Events;

public sealed record PermissionVersionIncrementedEvent(
    UserId UserId,
    TenantId TenantId,
    int NewVersion) : DomainEvent;
