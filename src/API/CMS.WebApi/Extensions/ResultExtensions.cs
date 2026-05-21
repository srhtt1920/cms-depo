using CMS.SharedKernel.Result;
using CMS.WebApi.ProblemDetails;

namespace CMS.WebApi.Extensions;

public static class ResultExtensions
{
    public static IResult ToHttpResult<T>(this Result<T> result) =>
        result.IsSuccess
            ? Results.Ok(result.Value)
            : result.Error.Type switch
            {
                ErrorType.NotFound => Results.NotFound(new NotFoundProblemDetails(result.Error.Message)),
                ErrorType.Validation => Results.UnprocessableEntity(new ValidationProblemDetails(result.Error.Message)),
                ErrorType.Conflict => Results.Conflict(new Microsoft.AspNetCore.Mvc.ProblemDetails { Title = result.Error.Code, Detail = result.Error.Message, Status = 409 }),
                ErrorType.Unauthorized => Results.Unauthorized(),
                ErrorType.Forbidden => Results.Json(new ForbiddenProblemDetails(result.Error.Message), statusCode: 403),
                _ => Results.Problem(result.Error.Message)
            };

    public static IResult ToHttpResult(this Result result) =>
        result.IsSuccess
            ? Results.NoContent()
            : result.Error.Type switch
            {
                ErrorType.NotFound => Results.NotFound(new NotFoundProblemDetails(result.Error.Message)),
                ErrorType.Validation => Results.UnprocessableEntity(new ValidationProblemDetails(result.Error.Message)),
                ErrorType.Conflict => Results.Conflict(new Microsoft.AspNetCore.Mvc.ProblemDetails { Title = result.Error.Code, Detail = result.Error.Message, Status = 409 }),
                ErrorType.Unauthorized => Results.Unauthorized(),
                ErrorType.Forbidden => Results.Json(new ForbiddenProblemDetails(result.Error.Message), statusCode: 403),
                _ => Results.Problem(result.Error.Message)
            };
}