namespace Template.Domain.Orders;

public sealed record OrderItemDraft(string ProductName, int Quantity, decimal UnitPrice);
