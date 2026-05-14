namespace Template.Application.Orders;

public sealed record CreateOrderRequest(
    string CustomerId,
    IReadOnlyCollection<CreateOrderItemRequest> Items);

public sealed record CreateOrderItemRequest(
    string ProductName,
    int Quantity,
    decimal UnitPrice);
