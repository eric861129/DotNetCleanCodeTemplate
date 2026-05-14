using Microsoft.Extensions.DependencyInjection;
using Template.Application.Common;
using Template.Application.Orders;

namespace Template.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IValidator<CreateOrderRequest>, CreateOrderRequestValidator>();
        services.AddScoped<IUseCase<CreateOrderRequest, Result<OrderResponse>>, CreateOrderUseCase>();
        services.AddScoped<IUseCase<GetOrderByIdRequest, Result<OrderResponse>>, GetOrderByIdUseCase>();
        services.AddScoped<IUseCase<GetOrdersRequest, Result<PagedResult<OrderResponse>>>, GetOrdersUseCase>();

        return services;
    }
}
