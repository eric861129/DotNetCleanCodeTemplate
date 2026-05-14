using Template.Application.Common;

namespace Template.Application.Orders;

public sealed class CreateOrderRequestValidator : IValidator<CreateOrderRequest>
{
    public ValidationResult Validate(CreateOrderRequest request)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(request.CustomerId))
        {
            errors.Add(new ValidationError(nameof(CreateOrderRequest.CustomerId), "Customer id is required."));
        }

        if (request.Items.Count == 0)
        {
            errors.Add(new ValidationError(nameof(CreateOrderRequest.Items), "An order must contain at least one item."));
        }

        var index = 0;
        foreach (var item in request.Items)
        {
            if (string.IsNullOrWhiteSpace(item.ProductName))
            {
                errors.Add(new ValidationError($"Items[{index}].ProductName", "Product name is required."));
            }

            if (item.Quantity <= 0)
            {
                errors.Add(new ValidationError($"Items[{index}].Quantity", "Quantity must be greater than zero."));
            }

            if (item.UnitPrice <= 0)
            {
                errors.Add(new ValidationError($"Items[{index}].UnitPrice", "Unit price must be greater than zero."));
            }

            index++;
        }

        return errors.Count == 0
            ? ValidationResult.Success()
            : ValidationResult.Failure(errors);
    }
}
