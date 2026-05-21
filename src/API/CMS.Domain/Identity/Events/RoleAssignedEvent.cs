using CMS.Domain.Common;
using CMS.Domain.Tenants;

namespace CMS.Domain.Identity.Events;

public sealed record RoleAssignedEvent(
    UserId UserId,
    TenantId TenantId,
    RoleId RoleId) : DomainEvent;
