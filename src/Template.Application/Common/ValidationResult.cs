namespace Template.Application.Common;

public sealed class ValidationResult
{
    private ValidationResult(IReadOnlyCollection<ValidationError> errors)
    {
        Errors = errors;
    }

    public bool IsValid => Errors.Count == 0;

    public IReadOnlyCollection<ValidationError> Errors { get; }

    public static ValidationResult Success() => new([]);

    public static ValidationResult Failure(IReadOnlyCollection<ValidationError> errors) => new(errors);
}
