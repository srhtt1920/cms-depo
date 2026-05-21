using CMS.Domain.Common;
using CMS.Domain.Tenants;

namespace CMS.Domain.Pages.Events;

public sealed record PageMovedEvent(
   PageId PageId,
   TenantId TenantId,
   PageId? OldParentId,
   PageId? NewParentId) : DomainEvent;
