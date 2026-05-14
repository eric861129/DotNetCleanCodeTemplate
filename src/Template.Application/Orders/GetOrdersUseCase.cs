using Template.Application.Common;

namespace Template.Application.Orders;

public sealed record GetOrdersRequest(int Page = 1, int PageSize = 20);

public sealed class GetOrdersUseCase(IOrderRepository orderRepository)
    : IUseCase<GetOrdersRequest, Result<PagedResult<OrderResponse>>>
{
    private const int MaxPageSize = 100;

    public async Task<Result<PagedResult<OrderResponse>>> HandleAsync(GetOrdersRequest request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, MaxPageSize);
        var orders = await orderRepository.GetPagedAsync(page, pageSize, cancellationToken);
        var totalCount = await orderRepository.CountAsync(cancellationToken);
        var response = new PagedResult<OrderResponse>(
            orders.Select(order => order.ToResponse()).ToList(),
            totalCount,
            page,
            pageSize);

        return Result<PagedResult<OrderResponse>>.Success(response);
    }
}
