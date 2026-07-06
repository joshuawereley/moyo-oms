namespace Moyo.Oms.Application.Abstractions.Persistence;

/// <summary>
/// Commits changes made through repositories as one atomic unit of work.
/// </summary>

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
