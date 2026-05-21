using CMS.Domain.Common;

namespace CMS.Domain.Tenants.Events;

public sealed record TenantCreatedEvent(TenantId TenantId, string Name) : DomainEvent;
