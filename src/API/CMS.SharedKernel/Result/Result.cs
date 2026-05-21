namespace CMS.SharedKernel.Result;

public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("Success result cannot have an error.");
        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException("Failure result must have an error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);
    public static Result<TValue> Failure<TValue>(Error error) => new(default!, false, error);

    public static TResponse CreateFailure<TResponse>(Error error) where TResponse : Result
    {
        if (typeof(TResponse) == typeof(Result))
            return (TResponse)(object)Failure(error);

        // Result<T> için: Failure<T>(error)
        // GetMethod'a tip listesi vererek ambiguity çözülür
        var valueType = typeof(TResponse).GetGenericArguments()[0];

        var method = typeof(Result).GetMethod(
            name: nameof(Failure),
            genericParameterCount: 1,
            bindingAttr: System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static,
            binder: null,
            types: [typeof(Error)],
            modifiers: null)!
            .MakeGenericMethod(valueType);

        return (TResponse)method.Invoke(null, [error])!;
    }
}

public sealed class Result<TValue> : Result
{
    private readonly TValue _value;

    internal Result(TValue value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    public TValue Value => IsSuccess
        ? _value
        : throw new InvalidOperationException("Failure result has no value.");

    public static implicit operator Result<TValue>(TValue value) => Success(value);

    public TOut Match<TOut>(
        Func<TValue, TOut> onSuccess,
        Func<Error, TOut> onFailure) =>
        IsSuccess ? onSuccess(Value) : onFailure(Error);
}
