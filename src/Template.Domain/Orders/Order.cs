using Template.Domain.Common;

namespace Template.Domain.Orders;

public sealed class Order
{
    private readonly List<OrderItem> _items = [];

    private Order()
    {
        CustomerId = string.Empty;
    }

    private Order(Guid id, string customerId, DateTimeOffset createdAt)
    {
        Id = id;
        CustomerId = customerId;
        Status = OrderStatus.Pending;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    public string CustomerId { get; private set; }

    public OrderStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? PaidAt { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public decimal TotalAmount => _items.Sum(item => item.LineTotal);

    public static Order Create(string customerId, IEnumerable<OrderItemDraft> items, DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(customerId))
        {
            throw new DomainException("Customer id is required.");
        }

        var itemList = items.Select(item => OrderItem.Create(item.ProductName, item.Quantity, item.UnitPrice)).ToList();
        if (itemList.Count == 0)
        {
            throw new DomainException("An order must contain at least one item.");
        }

        var order = new Order(Guid.NewGuid(), customerId.Trim(), createdAt);
        order._items.AddRange(itemList);

        return order;
    }

    public void MarkAsPaid()
    {
        if (Status != OrderStatus.Pending)
        {
            throw new DomainException("Only pending orders can be marked as paid.");
        }

        Status = OrderStatus.Paid;
        PaidAt = DateTimeOffset.UtcNow;
    }
}
