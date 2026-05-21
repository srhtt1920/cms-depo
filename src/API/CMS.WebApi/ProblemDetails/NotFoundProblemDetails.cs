namespace CMS.WebApi.ProblemDetails;

public sealed class NotFoundProblemDetails : Microsoft.AspNetCore.Mvc.ProblemDetails
{
    public NotFoundProblemDetails() { }

    public NotFoundProblemDetails(string message)
    {
        Title  = "Not found";
        Detail = message;
        Status = StatusCodes.Status404NotFound;
        Type   = "https://tools.ietf.org/html/rfc7231#section-6.5.4";
    }
}
