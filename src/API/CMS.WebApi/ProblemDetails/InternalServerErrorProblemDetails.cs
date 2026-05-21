
namespace CMS.WebApi.ProblemDetails;

public sealed class InternalServerErrorProblemDetails : Microsoft.AspNetCore.Mvc.ProblemDetails
{
    // [JsonConstructor] olmadan System.Text.Json primary constructor'ı bulamayabilir
    // ve base class'taki 'Detail' property ile çakışır.
    // Çözüm: parametresiz constructor + property assignment
    public InternalServerErrorProblemDetails() { }

    public InternalServerErrorProblemDetails(string message)
    {
        Title  = "Internal server error";
        Detail = message;
        Status = StatusCodes.Status500InternalServerError;
        Type   = "https://tools.ietf.org/html/rfc7231#section-6.6.1";
    }
}
