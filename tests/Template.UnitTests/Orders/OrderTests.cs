using Template.Domain.Orders;
using DomainException = Template.Domain.Common.DomainException;

namespace Template.UnitTests.Orders;

public sealed class OrderTests
{
    [Fact]
    public void CreateCalculatesTotalAmountFromItems()
    {
        var items = new[]
        {
            new OrderItemDraft("Clean Code", 2, 30m),
            new OrderItemDraft("Refactoring", 1, 45.5m)
        };

        var order = Order.Create("customer-001", items, DateTimeOffset.UtcNow);

        Assert.Equal(105.5m, order.TotalAmount);
    }

    [Fact]
    public void CreateRejectsOrdersWithoutItems()
    {
        var exception = Assert.Throws<DomainException>(() =>
            Order.Create("customer-001", [], DateTimeOffset.UtcNow));

        Assert.Equal("An order must contain at least one item.", exception.Message);
    }

    [Fact]
    public void MarkAsPaidMovesPendingOrderToPaid()
    {
        var order = Order.Create(
            "customer-001",
            [new OrderItemDraft("Clean Code", 1, 30m)],
            DateTimeOffset.UtcNow);

        order.MarkAsPaid();

        Assert.Equal(OrderStatus.Paid, order.Status);
    }
}
