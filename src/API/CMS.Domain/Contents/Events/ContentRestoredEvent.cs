using CMS.Domain.Common;
using CMS.Domain.Tenants;

namespace CMS.Domain.Contents.Events;

public sealed record ContentRestoredEvent(
    ContentId ContentId,
    TenantId TenantId) : DomainEvent;
