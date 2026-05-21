using CMS.Domain.Common;

namespace CMS.Domain.Identity.Events;

public sealed record UserCreatedEvent(UserId UserId, string Email) : DomainEvent;
