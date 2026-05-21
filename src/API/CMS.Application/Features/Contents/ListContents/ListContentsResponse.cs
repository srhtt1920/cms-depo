using CMS.SharedKernel.Pagination;

namespace CMS.Application.Features.Contents.ListContents;

public sealed record ListContentsResponse(Paginate<ContentSummaryDto> Data);

/// <summary>
/// Title: default veya fallback çeviriden alýnýr.
/// Languages: içeriðin mevcut çeviri dilleri.
/// </summary>
public sealed record ContentSummaryDto(
    Guid Id,
    string Slug,
    string Status,
    string ContentType,
    string Title,
    List<string> Languages,
    DateTime CreatedAt,
    DateTime? UpdatedAt);