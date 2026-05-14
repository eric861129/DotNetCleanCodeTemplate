using System.Text.Json;
using Template.Application.Common;
using Template.Domain.Orders;

namespace Template.Application.Orders;

public sealed class CreateOrderUseCase(
    IOrderRepository orderRepository,
    IOutboxMessageRepository outboxMessageRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IUseCase<CreateOrderRequest, Result<OrderResponse>>
{
    public async Task<Result<OrderResponse>> HandleAsync(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var now = timeProvider.GetUtcNow();
            var itemDrafts = request.Items.Select(item =>
                new OrderItemDraft(item.ProductName, item.Quantity, item.UnitPrice));

            var order = Order.Create(request.CustomerId, itemDrafts, now);
            var payload = JsonSerializer.Serialize(new OrderCreatedMessage(order.Id, order.CustomerId, order.TotalAmount, now));
            var outboxMessage = OutboxMessage.Create("OrderCreated", payload, now);

            await orderRepository.AddAsync(order, cancellationToken);
            await outboxMessageRepository.AddAsync(outboxMessage, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<OrderResponse>.Success(order.ToResponse());
        }
        catch (Template.Domain.Common.DomainException exception)
        {
            return Result<OrderResponse>.Failure(exception.Message);
        }
    }

    private sealed record OrderCreatedMessage(Guid OrderId, string CustomerId, decimal TotalAmount, DateTimeOffset OccurredAt);
}
