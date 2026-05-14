using Template.Application.Common;
using Template.Application.Orders;

namespace Template.UnitTests.Orders;

public sealed class CreateOrderRequestValidatorTests
{
    private readonly CreateOrderRequestValidator _validator = new();

    [Fact]
    public void ValidateRejectsEmptyCustomerId()
    {
        var request = new CreateOrderRequest(
            "",
            [new CreateOrderItemRequest("Clean Code", 1, 30m)]);

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateOrderRequest.CustomerId));
    }

    [Fact]
    public void ValidateRejectsEmptyItems()
    {
        var request = new CreateOrderRequest("customer-001", []);

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateOrderRequest.Items));
    }

    [Fact]
    public void ValidateRejectsInvalidItemValues()
    {
        var request = new CreateOrderRequest(
            "customer-001",
            [new CreateOrderItemRequest("", 0, 0m)]);

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Items[0].ProductName");
        Assert.Contains(result.Errors, error => error.PropertyName == "Items[0].Quantity");
        Assert.Contains(result.Errors, error => error.PropertyName == "Items[0].UnitPrice");
    }
}
