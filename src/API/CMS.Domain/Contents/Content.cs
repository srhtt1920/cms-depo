using CMS.Domain.Common;
using CMS.Domain.Contents.Events;
using CMS.Domain.Identity;
using CMS.Domain.Tenants;

namespace CMS.Domain.Contents;

public sealed class Content : AggregateRoot<ContentId>, ITenantEntity, ISoftDeletable
{
    private readonly List<ContentTranslation> _translations = [];
    private readonly List<ContentSection> _sections = [];

    public TenantId TenantId { get; private set; } = default!;
    public Slug Slug { get; private set; } = default!;
    public ContentType ContentType { get; private set; } = ContentType.Page;
    public ContentStatus Status { get; private set; } = ContentStatus.Draft;
    public UserId? AuthorId { get; private set; }
    public PublishSchedule? Schedule { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    Guid ITenantEntity.TenantId => TenantId.Value;
    public ApprovalInfo? Approval { get; private set; }


    public IReadOnlyList<ContentTranslation> Translations => _translations.AsReadOnly();
    public IReadOnlyList<ContentSection> Sections => _sections.AsReadOnly();

    private Content() { }

    public void SubmitForApproval(UserId submittedBy, string? note = null)
    {
        if (Status is ContentStatus.Published or ContentStatus.Archived)
            throw new InvalidOperationException("Bu durumdaki içerik onaya gönderilemez.");
        Status = ContentStatus.PendingApproval;
        Approval = ApprovalInfo.Pending(submittedBy, note);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Approve(UserId approvedBy, string? comment = null)
    {
        if (Status != ContentStatus.PendingApproval)
            throw new InvalidOperationException("Onaylanacak durumda değil.");
        Approval = Approval!.Accept(approvedBy, comment);
        Status = ContentStatus.Draft; // veya otomatik Publish istenebilir
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reject(UserId rejectedBy, string reason)
    {
        if (Status != ContentStatus.PendingApproval)
            throw new InvalidOperationException("Reddedilecek durumda değil.");
        Approval = Approval!.Reject(rejectedBy, reason);
        Status = ContentStatus.Rejected;
        UpdatedAt = DateTime.UtcNow;
    }

    // ── Factory ──────────────────────────────────────────────────────────────
    public static Content Create(
        TenantId tenantId,
        string slug,
        string languageCode,
        string title,
        string body,
        string? metaTitle = null,
        string? metaDescription = null,
        ContentType contentType = ContentType.Page,
        UserId? authorId = null)
    {
        var content = new Content
        {
            Id = ContentId.New(),
            TenantId = tenantId,
            Slug = Slug.Create(slug),
            ContentType = contentType,
            AuthorId = authorId,
            Status = ContentStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };
        content._translations.Add(ContentTranslation.Create(
            content.Id, languageCode, title, body, metaTitle, metaDescription));
        content.AddDomainEvent(new ContentCreatedEvent(content.Id, tenantId, slug));
        return content;
    }

    // ── Yazar ────────────────────────────────────────────────────────────────
    public void SetAuthor(UserId authorId)
    {
        AuthorId = authorId;
        UpdatedAt = DateTime.UtcNow;
    }

    // ── Çeviri ───────────────────────────────────────────────────────────────
    public void AddTranslation(string languageCode, string title, string body,
        string? metaTitle = null, string? metaDescription = null)
    {
        if (_translations.Any(t => t.LanguageCode == languageCode.ToLowerInvariant()))
            throw new InvalidOperationException($"Translation for '{languageCode}' already exists.");
        _translations.Add(ContentTranslation.Create(Id, languageCode, title, body, metaTitle, metaDescription));
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new ContentTranslationAddedEvent(Id, languageCode));
    }

    // ── Section ──────────────────────────────────────────────────────────────
    public ContentSection AddSection(
        string name, int order,
        ContentSectionId? parentSectionId = null,
        string? cssClass = null,
        SectionAnimationSettings? animation = null)
    {
        var section = ContentSection.Create(Id, name, order, parentSectionId, cssClass, animation);
        _sections.Add(section);
        UpdatedAt = DateTime.UtcNow;
        return section;
    }

    public void RemoveSection(ContentSectionId sectionId)
    {
        RemoveSectionRecursive(sectionId);
        UpdatedAt = DateTime.UtcNow;
    }

    private void RemoveSectionRecursive(ContentSectionId sectionId)
    {
        var children = _sections.Where(s => s.ParentSectionId == sectionId).ToList();
        foreach (var child in children) RemoveSectionRecursive(child.Id);
        _sections.RemoveAll(s => s.Id == sectionId);
    }

    // ── Zamanlama ─────────────────────────────────────────────────────────────
    public void SetSchedule(PublishSchedule schedule)
    {
        if (Status == ContentStatus.Archived)
            throw new InvalidOperationException("Archived content cannot be scheduled.");
        Schedule = schedule;
        Status = schedule.IsScheduled(DateTime.UtcNow)
            ? ContentStatus.Scheduled : ContentStatus.Draft;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ClearSchedule()
    {
        Schedule = null;
        if (Status == ContentStatus.Scheduled) Status = ContentStatus.Draft;
        UpdatedAt = DateTime.UtcNow;
    }

    // ── Yayın ─────────────────────────────────────────────────────────────────
    public void Publish()
    {
        if (Status == ContentStatus.Archived)
            throw new InvalidOperationException("Archived content cannot be published.");
        if (!_translations.Any())
            throw new InvalidOperationException("Content must have at least one translation.");
        foreach (var t in _translations.Where(t => !t.IsPublished)) t.Publish();
        Status = ContentStatus.Published;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new ContentPublishedEvent(Id, TenantId));
    }

    public void Unpublish()
    {
        if (Status != ContentStatus.Published)
            throw new InvalidOperationException("Only published content can be unpublished.");
        foreach (var t in _translations) t.Unpublish();
        Status = ContentStatus.Draft;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new ContentUnpublishedEvent(Id, TenantId)); // ← YENİ
    }

    public void Expire()
    {
        if (Status == ContentStatus.Published) Status = ContentStatus.Expired;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Archive()
    {
        Status = ContentStatus.Archived;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        DeletedAt = DateTime.UtcNow;
        AddDomainEvent(new ContentSoftDeletedEvent(Id, TenantId));
    }

    public void Restore()
    {
        DeletedAt = null;
        AddDomainEvent(new ContentRestoredEvent(Id, TenantId)); // ← YENİ
    }
}

