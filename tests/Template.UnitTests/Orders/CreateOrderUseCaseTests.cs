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
            new CreateOrderRequestValidator(),
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

    [Fact]
    public async Task HandleReturnsValidationErrorWithoutWritingRepositories()
    {
        var orderRepository = new InMemoryOrderRepository();
        var outboxRepository = new InMemoryOutboxMessageRepository();
        var unitOfWork = new InMemoryUnitOfWork();
        var useCase = new CreateOrderUseCase(
            new CreateOrderRequestValidator(),
            orderRepository,
            outboxRepository,
            unitOfWork,
            TimeProvider.System);

        var request = new CreateOrderRequest("", []);

        var result = await useCase.HandleAsync(request, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Empty(orderRepository.Orders);
        Assert.Empty(outboxRepository.Messages);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task GetOrdersReturnsPagedResult()
    {
        var orderRepository = new InMemoryOrderRepository();
        var useCase = new GetOrdersUseCase(orderRepository);
        var firstOrder = Order.Create(
            "customer-001",
            [new OrderItemDraft("Clean Code", 1, 30m)],
            DateTimeOffset.UtcNow);
        var secondOrder = Order.Create(
            "customer-002",
            [new OrderItemDraft("Refactoring", 1, 45m)],
            DateTimeOffset.UtcNow);
        orderRepository.Orders.AddRange([firstOrder, secondOrder]);

        var result = await useCase.HandleAsync(new GetOrdersRequest(1, 1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TotalCount);
        Assert.Equal(1, result.Value.Page);
        Assert.Equal(1, result.Value.PageSize);
        Assert.Single(result.Value.Items);
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

        public Task<IReadOnlyList<Order>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<Order>>(Orders
                .OrderByDescending(order => order.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList());
        }

        public Task<int> CountAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Orders.Count);
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
