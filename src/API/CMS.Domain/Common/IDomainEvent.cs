using MediatR;

namespace CMS.Domain.Common;

/// <summary>
/// Domain event base interface.
/// EventId ve OccurredOn record implementasyonlarında sabittir — her erişimde yeni değer üretmez.
/// </summary>
public interface IDomainEvent : INotification
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
}

/// <summary>
/// Concrete domain event'ler bu base record'dan türemeli.
/// EventId ve OccurredOn oluşturulma anında bir kez set edilir.
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
