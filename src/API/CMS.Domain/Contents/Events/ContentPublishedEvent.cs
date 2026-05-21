using CMS.Domain.Common;
using CMS.Domain.Tenants;

namespace CMS.Domain.Contents.Events;

public sealed record ContentPublishedEvent(
    ContentId ContentId,
    TenantId TenantId) : DomainEvent;
