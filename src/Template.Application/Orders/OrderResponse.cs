namespace Template.Application.Orders;

public sealed record OrderResponse(
    Guid Id,
    string CustomerId,
    string Status,
    decimal TotalAmount,
    DateTimeOffset CreatedAt,
    IReadOnlyCollection<OrderItemResponse> Items);

public sealed record OrderItemResponse(
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);
