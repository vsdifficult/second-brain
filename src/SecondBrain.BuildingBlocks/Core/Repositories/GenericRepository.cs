// src/SecondBrain.BuildingBlocks/Core/Repositories/GenericRepository.cs
using Microsoft.EntityFrameworkCore;
using SecondBrain.BuildingBlocks.Core.Entities;
using SecondBrain.BuildingBlocks.EFCore;
using SecondBrain.BuildingBlocks.Messaging.Kafka.Abstractions;
using System.Linq.Expressions;

namespace SecondBrain.BuildingBlocks.Core.Repositories;

public class GenericRepository<T, TId> : IRepository<T, TId> where T : BaseEntity
{
    protected readonly BaseBbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(BaseBbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
        => await _dbSet.FindAsync(new object[] { id }, cancellationToken);

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbSet.ToListAsync(cancellationToken);

    public virtual async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => await _dbSet.Where(predicate).ToListAsync(cancellationToken);

    public virtual async Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    public virtual Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public virtual async Task DeleteAsync(TId id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
            _dbSet.Remove(entity);
    }

    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => await _dbSet.AnyAsync(predicate, cancellationToken);

    public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    public virtual void EnqueueOutboxMessage<TEvent>(string topic, string key, TEvent @event)
        where TEvent : Event
        => _context.EnqueueOutboxMessage(topic, key, @event);
}