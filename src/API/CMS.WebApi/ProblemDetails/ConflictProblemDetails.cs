namespace CMS.WebApi.ProblemDetails;

public sealed class ConflictProblemDetails : Microsoft.AspNetCore.Mvc.ProblemDetails
{
    public ConflictProblemDetails() { }

    public ConflictProblemDetails(string code, string message)
    {
        Title = code;
        Detail = message;
        Status = StatusCodes.Status409Conflict;
        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8";
    }
}
