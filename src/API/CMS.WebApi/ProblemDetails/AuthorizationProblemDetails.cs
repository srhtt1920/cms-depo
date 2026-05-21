
namespace CMS.WebApi.ProblemDetails;

public sealed class AuthorizationProblemDetails : Microsoft.AspNetCore.Mvc.ProblemDetails
{
    public AuthorizationProblemDetails() { }

    public AuthorizationProblemDetails(string message)
    {
        Title = "Unauthorized";
        Detail = message;
        Status = StatusCodes.Status401Unauthorized;
        Type = "https://tools.ietf.org/html/rfc7235#section-3.1";
    }
}
