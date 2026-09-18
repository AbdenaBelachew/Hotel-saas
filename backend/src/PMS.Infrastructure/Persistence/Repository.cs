using Microsoft.EntityFrameworkCore;
using PMS.Application.Common.Interfaces;

namespace PMS.Infrastructure.Persistence;

public class Repository<T>(ApplicationDbContext dbContext) : IRepository<T> where T : class
{
    private readonly DbSet<T> _set = dbContext.Set<T>();

    public IQueryable<T> Query() => _set.AsNoTracking();

    public IQueryable<T> QueryUnfiltered() => _set.IgnoreQueryFilters().AsNoTracking();

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _set.FindAsync([id], cancellationToken);

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default) =>
        await _set.AddAsync(entity, cancellationToken);

    public void Update(T entity) => _set.Update(entity);

    public void Remove(T entity) => _set.Remove(entity);
}
