using Infrastructure.Base;
using Microsoft.EntityFrameworkCore.Storage;

namespace AuthService.Base;

public class UnitOfWork : IUnitOfWork , IDisposable
{
    private readonly AppDbContext _context;
    private IDbContextTransaction _transaction;
    private bool _disposed = false;
    private readonly Dictionary<Type , object> _repositories = new();

    public UnitOfWork(AppDbContext context){
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public IGenericRepository<T> Repository<T>() where T : class
    {
        if(_repositories.ContainsKey(typeof(T))){

            return _repositories[typeof(T)] as IGenericRepository<T>;
        }

        var repository = new GenericRepository<T>(_context);
        _repositories[typeof(T)] = repository;
        return repository;
    
    }

    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }
    public async Task BeginTransactionAsync()
    {
        if (_transaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress.");
        }

        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No transaction in progress.");
        }

        try
        {
            await _context.SaveChangesAsync();
            await _transaction.CommitAsync();
        }
        catch
        {
            await RollbackTransactionAsync(); // Ensure this is awaited
            throw;
        }
        finally
        {
            await _transaction.DisposeAsync(); // Ensure proper asynchronous disposal
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No transaction in progress.");
        }

        try
        {
            await _transaction.RollbackAsync(); // Ensure this is awaited
        }
        finally
        {
            await _transaction.DisposeAsync(); // Ensure proper asynchronous disposal
            _transaction = null;
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                foreach (var repository in _repositories.Values)
                {
                    if (repository is IDisposable disposableRepository)
                    {
                        disposableRepository.Dispose();
                    }
                }
        
                _context.Dispose();
                _transaction?.Dispose();
            }
        
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

}