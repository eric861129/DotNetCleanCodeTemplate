namespace Template.Application.Common;

public sealed class Result<T>
{
    private Result(T value)
    {
        Value = value;
        Error = string.Empty;
        IsSuccess = true;
    }

    private Result(string error)
    {
        Value = default!;
        Error = error;
        IsSuccess = false;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public T Value { get; }

    public string Error { get; }

    public static Result<T> Success(T value) => new(value);

    public static Result<T> Failure(string error) => new(error);
}
