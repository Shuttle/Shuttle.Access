using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Shuttle.Core.Data;

namespace Shuttle.Access.Tests.Integration;

public class MockDatabaseContext : IDatabaseContext
{
    public void Dispose()
    {
        Disposed?.Invoke(this, EventArgs.Empty);

        if (HasTransaction)
        {
            TransactionRolledBack?.Invoke(this, new(null!));
        }
    }

    public async ValueTask DisposeAsync()
    {
        await ValueTask.CompletedTask;
    }

    public async Task<IDatabaseContext> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.Unspecified)
    {
        await Task.CompletedTask;

        TransactionStarted?.Invoke(this, new(null!));

        return this;
    }

    public async Task CommitTransactionAsync()
    {
        TransactionCommitted?.Invoke(this, new(null!));

        await Task.CompletedTask;
    }

    public async Task<DbCommand> CreateCommandAsync(IQuery query)
    {
        await Task.CompletedTask;

        return new SqlCommand();
    }

    public IDbConnection GetDbConnection()
    {
        return new SqlConnection();
    }

    public bool HasTransaction { get; } = false;
    public bool IsActive { get; } = true;
    public string Name { get; } = "Fixture";
    public string ProviderName { get; } = "None";
    public DbTransaction? Transaction { get; } = null;
    
    public event EventHandler<EventArgs>? Disposed;
    public event EventHandler<TransactionEventArgs>? TransactionCommitted;
    public event EventHandler<TransactionEventArgs>? TransactionRolledBack;
    public event EventHandler<TransactionEventArgs>? TransactionStarted;
}

