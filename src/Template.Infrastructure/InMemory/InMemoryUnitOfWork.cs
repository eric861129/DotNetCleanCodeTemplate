using Template.Application.Common;

namespace Template.Infrastructure.InMemory;

public sealed class InMemoryUnitOfWork : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(0);
    }
}
