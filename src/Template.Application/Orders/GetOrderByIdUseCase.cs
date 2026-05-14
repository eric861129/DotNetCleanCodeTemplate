using Template.Application.Common;

namespace Template.Application.Orders;

public sealed record GetOrderByIdRequest(Guid Id);

public sealed class GetOrderByIdUseCase(IOrderRepository orderRepository)
    : IUseCase<GetOrderByIdRequest, Result<OrderResponse>>
{
    public async Task<Result<OrderResponse>> HandleAsync(GetOrderByIdRequest request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.Id, cancellationToken);

        return order is null
            ? Result<OrderResponse>.Failure(Error.NotFound("Orders.NotFound", "Order was not found."))
            : Result<OrderResponse>.Success(order.ToResponse());
    }
}
