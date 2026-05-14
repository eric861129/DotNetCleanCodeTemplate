namespace Template.Application.Common;

public interface IValidator<in TRequest>
{
    ValidationResult Validate(TRequest request);
}
