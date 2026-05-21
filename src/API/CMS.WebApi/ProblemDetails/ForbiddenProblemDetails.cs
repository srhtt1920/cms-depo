
namespace CMS.WebApi.ProblemDetails;

public sealed class ForbiddenProblemDetails : Microsoft.AspNetCore.Mvc.ProblemDetails
{
    public ForbiddenProblemDetails() { }

    public ForbiddenProblemDetails(string message)
    {
        Title  = "Forbidden";
        Detail = message;
        Status = StatusCodes.Status403Forbidden;
        Type   = "https://tools.ietf.org/html/rfc7231#section-6.5.3";
    }
}
