namespace Domain.Interfaces
{
    /// <summary>
    /// Unit of Work para gestionar transacciones
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        ITransactionRepository Expenses { get; }
        IFixedExpenseRepository FixedExpenses { get; }
        IBudgetRepository Budgets { get; }
        ICategoryRepository Categories { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
        Task<bool> HasChangesAsync();
    }
}