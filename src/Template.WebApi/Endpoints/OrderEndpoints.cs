using Template.Application.Common;
using Template.Application.Orders;
using Template.WebApi.Http;

namespace Template.WebApi.Endpoints;

public static class OrderEndpoints
{
    private const string ApiVersion = "v1";
    private const string OrdersRoutePrefix = $"/api/{ApiVersion}/orders";

    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(OrdersRoutePrefix)
            .WithTags("Orders")
            .WithGroupName(ApiVersion)
            .RequireRateLimiting("global");

        //#if (useJwt)
        group.RequireAuthorization();
        //#endif

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
        //#if (useJwt)
        .ProducesProblem(StatusCodes.Status401Unauthorized);
        //#endif

        group.MapPost("/", async (
            CreateOrderRequest request,
            IUseCase<CreateOrderRequest, Result<OrderResponse>> useCase,
            CancellationToken cancellationToken) =>
        {
            var result = await useCase.HandleAsync(request, cancellationToken);

            return result.ToHttpResult(order => Results.Created($"{OrdersRoutePrefix}/{order.Id}", order));
        })
        .WithName("CreateOrder")
        .Produces<OrderResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        //#if (useJwt)
        .ProducesProblem(StatusCodes.Status401Unauthorized);
        //#endif

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
        //#if (useJwt)
        .ProducesProblem(StatusCodes.Status401Unauthorized);
        //#endif

        return app;
    }
}
