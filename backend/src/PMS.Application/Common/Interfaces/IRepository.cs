namespace PMS.Application.Common.Interfaces;

public interface IRepository<T> where T : class
{
    /// <summary>Untracked, tenant/soft-delete filters (from the DbContext's global query filters) already applied.</summary>
    IQueryable<T> Query();

    /// <summary>
    /// Bypasses the tenant global query filter. Reserved for system/pre-authentication contexts
    /// (login, registration, password reset, refresh-token lookup) that must find a single record
    /// belonging to any tenant by a unique key, before a tenant context exists.
    /// </summary>
    IQueryable<T> QueryUnfiltered();

    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Remove(T entity);
}
