using CMS.Domain.Common;

namespace CMS.Domain.Contents.Events;

public sealed record ContentTranslationAddedEvent(
    ContentId ContentId,
    string LanguageCode) : DomainEvent;
