using Template.Application.Common;
using Template.Application.Orders;
using Template.Domain.Orders;

namespace Template.UnitTests.Orders;

public sealed class CreateOrderUseCaseTests
{
    [Fact]
    public async Task HandleCreatesOrderAndOutboxMessage()
    {
        var orderRepository = new InMemoryOrderRepository();
        var outboxRepository = new InMemoryOutboxMessageRepository();
        var unitOfWork = new InMemoryUnitOfWork();
        var useCase = new CreateOrderUseCase(
            orderRepository,
            outboxRepository,
            unitOfWork,
            TimeProvider.System);

        var request = new CreateOrderRequest(
            "customer-001",
            [new CreateOrderItemRequest("Clean Code", 2, 30m)]);

        var result = await useCase.HandleAsync(request, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
        Assert.Equal(60m, result.Value.TotalAmount);
        Assert.Single(orderRepository.Orders);
        Assert.Single(outboxRepository.Messages);
        Assert.Equal("OrderCreated", outboxRepository.Messages[0].Type);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    private sealed class InMemoryOrderRepository : IOrderRepository
    {
        public List<Order> Orders { get; } = [];

        public Task AddAsync(Order order, CancellationToken cancellationToken)
        {
            Orders.Add(order);
            return Task.CompletedTask;
        }

        public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(Orders.SingleOrDefault(order => order.Id == id));
        }
    }

    private sealed class InMemoryOutboxMessageRepository : IOutboxMessageRepository
    {
        public List<OutboxMessage> Messages { get; } = [];

        public Task AddAsync(OutboxMessage message, CancellationToken cancellationToken)
        {
            Messages.Add(message);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<OutboxMessage>>(Messages.Where(message => message.ProcessedAt is null).ToList());
        }
    }

    private sealed class InMemoryUnitOfWork : IUnitOfWork
    {
        public int SaveChangesCallCount { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveChangesCallCount++;
            return Task.FromResult(1);
        }
    }
}
