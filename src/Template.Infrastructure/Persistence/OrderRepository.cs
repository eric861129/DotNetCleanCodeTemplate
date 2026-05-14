using Microsoft.EntityFrameworkCore;
using Template.Application.Orders;
using Template.Domain.Orders;

namespace Template.Infrastructure.Persistence;

public sealed class OrderRepository(AppDbContext dbContext) : IOrderRepository
{
    public async Task AddAsync(Order order, CancellationToken cancellationToken)
    {
        await dbContext.Orders.AddAsync(order, cancellationToken);
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Orders
            .Include(order => order.Items)
            .FirstOrDefaultAsync(order => order.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var orders = await dbContext.Orders
            .Include(order => order.Items)
            .ToListAsync(cancellationToken);

        return orders
            .OrderByDescending(order => order.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public Task<int> CountAsync(CancellationToken cancellationToken)
    {
        return dbContext.Orders.CountAsync(cancellationToken);
    }
}
