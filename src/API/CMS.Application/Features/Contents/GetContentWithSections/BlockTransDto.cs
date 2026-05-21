namespace CMS.Application.Features.Contents.GetContentWithSections;

public sealed record BlockTransDto(
    string LanguageCode,
    string? Title,
    string? Body,
    string? AltText,
    string? LinkText);
