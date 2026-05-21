using CMS.Application.Common.Abstractions;
using CMS.Domain.Contents;
using CMS.Domain.Contents.Versioning;
using CMS.Domain.Identity;
using CMS.Domain.Tenants;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;
using System.Text.Json;

namespace CMS.Application.Features.Contents.RestoreVersion;

[RequirePermission("content.edit")]
public sealed class RestoreVersionHandler(
    IContentVersionRepository versionRepository,
    IContentRepository contentRepository,
    ITenantContext tenantContext,
    ICurrentUser currentUser)
    : IRequestHandler<RestoreVersionCommand, Result>
{
    public async Task<Result> Handle(RestoreVersionCommand request, CancellationToken ct)
    {
        var content = await contentRepository.GetByIdAsync(
            ContentId.From(request.ContentId), ct);
        if (content is null)
            return Result.Failure(Error.NotFound("Content.NotFound", "Content not found."));
        if (content.TenantId != TenantId.From(tenantContext.TenantId))
            return Result.Failure(Error.Forbidden("Content.WrongTenant", "Access denied."));

        var version = await versionRepository.GetByNumberAsync(
            ContentId.From(request.ContentId), request.VersionNumber, ct);
        if (version is null)
            return Result.Failure(Error.NotFound("Version.NotFound",
                $"Version {request.VersionNumber} not found."));

        // Snapshot'tan çevirileri çıkar ve güncelle
        var snap = JsonSerializer.Deserialize<ContentSnapshot>(version.Snapshot);
        if (snap is null)
            return Result.Failure(Error.Validation("Version.InvalidSnapshot", "Snapshot corrupt."));

        foreach (var t in snap.Translations)
        {
            var existing = content.Translations
                .FirstOrDefault(tr => tr.LanguageCode == t.LanguageCode);
            if (existing is not null)
                existing.Update(t.Title, t.Body, t.MetaTitle, t.MetaDescription);
            else
                content.AddTranslation(t.LanguageCode, t.Title, t.Body, t.MetaTitle, t.MetaDescription);
        }

        // Yeni version kaydet (restore işlemi de bir version)
        var latestNum = await versionRepository.GetLatestVersionNumberAsync(
            ContentId.From(request.ContentId), ct);
        var newVersion = ContentVersion.Create(
            ContentId.From(request.ContentId),
            latestNum + 1,
            version.Snapshot,
            UserId.From(currentUser.UserId),
            label: $"Restored from v{request.VersionNumber}");
        await versionRepository.AddAsync(newVersion, ct);

        await contentRepository.UpdateAsync(content, ct);
        return Result.Success();
    }

    private sealed record ContentSnapshot(List<TranslationSnapshot> Translations);
    private sealed record TranslationSnapshot(
        string LanguageCode, string Title, string Body,
        string? MetaTitle, string? MetaDescription);
}
