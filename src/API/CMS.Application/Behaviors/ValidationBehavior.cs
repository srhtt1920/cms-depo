using CMS.SharedKernel.Result;
using FluentValidation;
using MediatR;

namespace CMS.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (!validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var failures = validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(e => e is not null)
            .GroupBy(e => e.PropertyName)
            .Select(g => $"{g.Key}: {string.Join(", ", g.Select(e => e.ErrorMessage))}")
            .ToList();

        if (failures.Count == 0)
            return await next();

        var error = Error.Validation(
            "Validation.Failed",
            string.Join("; ", failures));

        // Reflection ambiguity yok — Result.CreateFailure<TResponse> kullan
        return Result.CreateFailure<TResponse>(error);
    }
}
