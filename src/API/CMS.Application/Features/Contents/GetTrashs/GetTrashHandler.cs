using CMS.Application.Common.Abstractions;
using CMS.Domain.Contents;
using CMS.Domain.Tenants;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.GetTrashs;

[RequirePermission("content.readDeleted")]
public sealed class GetTrashHandler(
    IContentRepository contentRepository,
    ITenantContext tenantContext)
    : IRequestHandler<GetTrashQuery, Result<List<TrashItemDto>>>
{
    public async Task<Result<List<TrashItemDto>>> Handle(
        GetTrashQuery request, CancellationToken ct)
    {
        var tenantId = TenantId.From(tenantContext.TenantId);
        var items = await contentRepository.GetTrashedAsync(tenantId, ct);

        var dtos = items.Select(c =>
        {
            var title = c.Translations.FirstOrDefault()?.Title ?? string.Empty;
            var langs = c.Translations.Select(t => t.LanguageCode).ToList();
            return new TrashItemDto(c.Id.Value, c.Slug.Value, title, langs, c.DeletedAt);
        }).ToList();

        return dtos;
    }
}
