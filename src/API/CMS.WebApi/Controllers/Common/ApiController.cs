using CMS.SharedKernel.Result;
using CMS.WebApi.ProblemDetails;
using Microsoft.AspNetCore.Mvc;
using ValidationProblemDetails = CMS.WebApi.ProblemDetails.ValidationProblemDetails;

namespace CMS.WebApi.Controllers.Common;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json", "application/x-msgpack")]
public abstract class ApiController : ControllerBase
{
    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Ok(result.Value);

        return result.Error.Type switch
        {
            ErrorType.NotFound => NotFound(new NotFoundProblemDetails(result.Error.Message)),
            ErrorType.Validation => UnprocessableEntity(new ValidationProblemDetails(result.Error.Message)),
            ErrorType.Conflict => Conflict(new Microsoft.AspNetCore.Mvc.ProblemDetails
            { Title = result.Error.Code, Detail = result.Error.Message, Status = 409 }),
            ErrorType.Unauthorized => Unauthorized(new AuthorizationProblemDetails(result.Error.Message)),
            ErrorType.Forbidden => StatusCode(403, new ForbiddenProblemDetails(result.Error.Message)),
            _ => StatusCode(500, new InternalServerErrorProblemDetails(result.Error.Message))
        };
    }

    protected IActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
            return NoContent();

        return result.Error.Type switch
        {
            ErrorType.NotFound => NotFound(new NotFoundProblemDetails(result.Error.Message)),
            ErrorType.Validation => UnprocessableEntity(new ValidationProblemDetails(result.Error.Message)),
            ErrorType.Conflict => Conflict(new Microsoft.AspNetCore.Mvc.ProblemDetails
            { Title = result.Error.Code, Detail = result.Error.Message, Status = 409 }),
            ErrorType.Unauthorized => Unauthorized(new AuthorizationProblemDetails(result.Error.Message)),
            ErrorType.Forbidden => StatusCode(403, new ForbiddenProblemDetails(result.Error.Message)),
            _ => StatusCode(500, new InternalServerErrorProblemDetails(result.Error.Message))
        };
    }
}
