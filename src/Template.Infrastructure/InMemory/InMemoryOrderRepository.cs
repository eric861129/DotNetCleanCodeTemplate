using Template.Application.Orders;
using Template.Domain.Orders;

namespace Template.Infrastructure.InMemory;

public sealed class InMemoryOrderRepository : IOrderRepository
{
    private readonly List<Order> _orders = [];

    public Task AddAsync(Order order, CancellationToken cancellationToken)
    {
        _orders.Add(order);

        return Task.CompletedTask;
    }

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return Task.FromResult(_orders.SingleOrDefault(order => order.Id == id));
    }

    public Task<IReadOnlyList<Order>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        IReadOnlyList<Order> orders = _orders
            .OrderByDescending(order => order.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult(orders);
    }

    public Task<int> CountAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(_orders.Count);
    }
}
