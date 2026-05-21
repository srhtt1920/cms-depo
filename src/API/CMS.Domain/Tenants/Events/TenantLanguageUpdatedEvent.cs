using CMS.Domain.Common;

namespace CMS.Domain.Tenants.Events;

public sealed record TenantLanguageUpdatedEvent(TenantId TenantId) : DomainEvent;
