using CMS.Domain.Common;
using CMS.Domain.Tenants;

namespace CMS.Domain.Pages.Events;

public sealed record PageCreatedEvent(
    PageId PageId,
    TenantId TenantId) : DomainEvent;