namespace Template.Application.Common;

public sealed record Error(
    string Code,
    string Message,
    ErrorType Type,
    IReadOnlyCollection<ValidationError>? ValidationErrors = null)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.None);

    public static Error Validation(IReadOnlyCollection<ValidationError> errors)
    {
        return new Error("Validation.Failed", "One or more validation errors occurred.", ErrorType.Validation, errors);
    }

    public static Error NotFound(string code, string message)
    {
        return new Error(code, message, ErrorType.NotFound);
    }

    public static Error Conflict(string code, string message)
    {
        return new Error(code, message, ErrorType.Conflict);
    }

    public static Error Domain(string code, string message)
    {
        return new Error(code, message, ErrorType.Domain);
    }
}
