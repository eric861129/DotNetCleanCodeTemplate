using Template.Domain.Orders;

namespace Template.Application.Orders;

internal static class OrderMapping
{
    public static OrderResponse ToResponse(this Order order)
    {
        return new OrderResponse(
            order.Id,
            order.CustomerId,
            order.Status.ToString(),
            order.TotalAmount,
            order.CreatedAt,
            order.Items.Select(item => new OrderItemResponse(
                item.ProductName,
                item.Quantity,
                item.UnitPrice,
                item.LineTotal)).ToList());
    }
}
