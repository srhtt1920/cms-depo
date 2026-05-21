namespace CMS.WebApi.ProblemDetails;

public sealed class ValidationProblemDetails : Microsoft.AspNetCore.Mvc.ProblemDetails
{
    public List<string> Errors { get; init; } = [];

    public ValidationProblemDetails() { }

    public ValidationProblemDetails(string message)
    {
        Title = "Validation failed";
        Detail = message;
        Status = StatusCodes.Status422UnprocessableEntity;
        Type = "https://tools.ietf.org/html/rfc4918#section-11.2";
    }

    public ValidationProblemDetails(IEnumerable<string> errors)
        : this(string.Join("; ", errors))
    {
        Errors = errors.ToList();
    }
}
