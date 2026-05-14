using Template.Application.Common;
using Template.Application.Orders;

namespace Template.WebApi.Endpoints;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders")
            .WithTags("Orders")
            .RequireAuthorization();

        group.MapPost("/", async (
            CreateOrderRequest request,
            IUseCase<CreateOrderRequest, Result<OrderResponse>> useCase,
            CancellationToken cancellationToken) =>
        {
            var result = await useCase.HandleAsync(request, cancellationToken);

            return result.IsSuccess
                ? Results.Created($"/api/orders/{result.Value.Id}", result.Value)
                : Results.BadRequest(new { message = result.Error });
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

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(new { message = result.Error });
        })
        .WithName("GetOrderById")
        .Produces<OrderResponse>()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}
