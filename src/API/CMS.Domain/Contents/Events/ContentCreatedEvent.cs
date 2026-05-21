using CMS.Domain.Common;
using CMS.Domain.Tenants;

namespace CMS.Domain.Contents.Events;

public sealed record ContentCreatedEvent(
    ContentId ContentId,
    TenantId TenantId,
    string Slug) : DomainEvent;
