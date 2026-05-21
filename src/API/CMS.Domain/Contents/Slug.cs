using CMS.Domain.Common;
using CMS.SharedKernel.Result;
using System.Text.RegularExpressions;

namespace CMS.Domain.Contents;

public sealed record Slug : ValueObject
{
    private static readonly Regex SlugRegex = new(
        @"^[a-z0-9]+(?:-[a-z0-9]+)*$",
        RegexOptions.Compiled);

    public string Value { get; }

    private Slug(string value) => Value = value;

    /// <summary>
    /// Slug oluşturur. Geçersizse exception fırlatmak yerine Result.Failure döner.
    /// </summary>
    public static Result<Slug> TryCreate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<Slug>(Error.Validation("Slug.Empty", "Slug cannot be empty."));

        if (value.Length > 200)
            return Result.Failure<Slug>(Error.Validation("Slug.TooLong", "Slug cannot exceed 200 characters."));

        var normalized = value.ToLowerInvariant().Trim();
        if (!SlugRegex.IsMatch(normalized))
            return Result.Failure<Slug>(Error.Validation("Slug.InvalidFormat",
                "Slug must contain only lowercase letters, numbers, and hyphens."));

        return new Slug(normalized);
    }

    /// <summary>
    /// EF Core value conversion ve migration için internal factory (sadece trusted sources).
    /// </summary>
    public static Slug Create(string value) => new(value.ToLowerInvariant().Trim());

    public static implicit operator string(Slug slug) => slug.Value;
    public override string ToString() => Value;
}
