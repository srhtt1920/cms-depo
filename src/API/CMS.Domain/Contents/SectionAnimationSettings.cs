using CMS.Domain.Common;

namespace CMS.Domain.Contents;

public sealed record SectionAnimationSettings : ValueObject
{
    public string? Type     { get; init; }   // "fade-in", "slide-up", "zoom"
    public int     Duration { get; init; } = 300;
    public int     Delay    { get; init; } = 0;
    public string  Easing   { get; init; } = "ease-out";

    public static SectionAnimationSettings None => new();
}
