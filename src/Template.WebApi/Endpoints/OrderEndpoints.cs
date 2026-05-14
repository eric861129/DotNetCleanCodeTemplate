using Template.Application.Common;
using Template.Application.Orders;
using Template.WebApi.Http;

namespace Template.WebApi.Endpoints;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders")
            .WithTags("Orders")
            .RequireAuthorization();

        group.MapGet("/", async (
            int page,
            int pageSize,
            IUseCase<GetOrdersRequest, Result<PagedResult<OrderResponse>>> useCase,
            CancellationToken cancellationToken) =>
        {
            var result = await useCase.HandleAsync(new GetOrdersRequest(page, pageSize), cancellationToken);

            return result.ToHttpResult(Results.Ok);
        })
        .WithName("GetOrders")
        .Produces<PagedResult<OrderResponse>>()
        .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPost("/", async (
            CreateOrderRequest request,
            IUseCase<CreateOrderRequest, Result<OrderResponse>> useCase,
            CancellationToken cancellationToken) =>
        {
            var result = await useCase.HandleAsync(request, cancellationToken);

            return result.ToHttpResult(order => Results.Created($"/api/orders/{order.Id}", order));
        })
        .WithName("CreateOrder")
        .Produces<OrderResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapGet("/{id:guid}", async (
            Guid id,
            IUseCase<GetOrderByIdRequest, Result<OrderResponse>> useCase,
            CancellationToken cancellationToken) =>
        {
            var result = await useCase.HandleAsync(new GetOrderByIdRequest(id), cancellationToken);

            return result.ToHttpResult(Results.Ok);
        })
        .WithName("GetOrderById")
        .Produces<OrderResponse>()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}
