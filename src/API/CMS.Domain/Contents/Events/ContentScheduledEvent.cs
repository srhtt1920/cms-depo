using CMS.Domain.Common;
using CMS.Domain.Tenants;

namespace CMS.Domain.Contents.Events;
public sealed record ContentScheduledEvent(ContentId ContentId, TenantId TenantId, DateTime PublishAt) : DomainEvent;
