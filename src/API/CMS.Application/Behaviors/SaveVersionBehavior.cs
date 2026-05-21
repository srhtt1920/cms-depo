using CMS.Application.Common.Abstractions;
using CMS.Application.Features.Contents.UpdateContent;
using CMS.Domain.Contents;
using CMS.Domain.Contents.Versioning;
using CMS.Domain.Identity;
using CMS.SharedKernel.Result;
using MediatR;
using System.Text.Json;

namespace CMS.Application.Behaviors;

// <summary>
/// UpdateContentCommand başarıyla tamamlandıktan sonra otomatik snapshot alır.
/// Sadece bu command için aktif olur, diğer request'leri bypass eder.
/// </summary>
public sealed class SaveVersionBehavior<TRequest, TResponse>(
    IContentRepository contentRepository,
    IContentVersionRepository versionRepository,
    ICurrentUser currentUser)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var response = await next();

        // Sadece UpdateContentCommand — diğerlerini bypass et
        if (request is not UpdateContentCommand cmd) return response;

        // Başarı kontrolü: Result ve Result<T> her ikisi için
        var isSuccess = response switch
        {
            Result r => r.IsSuccess,
            _ => false
        };
        if (!isSuccess) return response;

        try
        {
            var content = await contentRepository.GetByIdAsync(
                ContentId.From(cmd.ContentId), ct);
            if (content is null) return response;

            var snapshot = JsonSerializer.Serialize(new
            {
                translations = content.Translations.Select(t => new
                {
                    t.LanguageCode,
                    t.Title,
                    t.Body,
                    t.MetaTitle,
                    t.MetaDescription
                })
            });

            var latestNum = await versionRepository.GetLatestVersionNumberAsync(
                ContentId.From(cmd.ContentId), ct);

            var version = ContentVersion.Create(
                ContentId.From(cmd.ContentId),
                latestNum + 1,
                snapshot,
                UserId.From(currentUser.UserId),
                isPublished: content.Status == ContentStatus.Published);

            await versionRepository.AddAsync(version, ct);
        }
        catch
        {
            // Version kayıt hatası ana işlemi bozmasın — sessizce geç
        }

        return response;
    }
}