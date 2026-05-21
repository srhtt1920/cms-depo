using CMS.Domain.Common;
using CMS.SharedKernel.DynamicQuery;
using CMS.SharedKernel.Pagination;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace CMS.Infrastructure.Persistence.Repositories.Base;

public static class DynamicQueryExtensions
{
    public static IQueryable<T> ToDynamic<T>(this IQueryable<T> query, DynamicQuery dynamicQuery)
    {
        if (dynamicQuery.Filter is not null)
            query = query.Where(BuildFilterExpression(dynamicQuery.Filter));

        if (dynamicQuery.Sort is not null)
        {
            var sorts = dynamicQuery.Sort.ToList();
            if (sorts.Count > 0)
            {
                var orderExpression = string.Join(",", sorts.Select(s => $"{s.Field} {s.Dir}"));
                query = query.OrderBy(orderExpression);
            }
        }

        return query;
    }

    private static string BuildFilterExpression(Filter filter)
    {
        if (filter.Filters?.Any() == true)
        {
            var logic = filter.Logic ?? "and";
            var parts = filter.Filters.Select(BuildFilterExpression);
            return $"({string.Join($" {logic} ", parts)})";
        }

        return filter.Operator.ToLower() switch
        {
            "eq" => $"{filter.Field} == \"{filter.Value}\"",
            "neq" => $"{filter.Field} != \"{filter.Value}\"",
            "contains" => $"{filter.Field}.Contains(\"{filter.Value}\")",
            "startswith" => $"{filter.Field}.StartsWith(\"{filter.Value}\")",
            "endswith" => $"{filter.Field}.EndsWith(\"{filter.Value}\")",
            "gt" => $"{filter.Field} > {filter.Value}",
            "gte" => $"{filter.Field} >= {filter.Value}",
            "lt" => $"{filter.Field} < {filter.Value}",
            "lte" => $"{filter.Field} <= {filter.Value}",
            "isnull" => $"{filter.Field} == null",
            "isnotnull" => $"{filter.Field} != null",
            _ => $"{filter.Field} == \"{filter.Value}\""
        };
    }
}

public abstract class EfRepositoryBase<TEntity, TEntityId, TContext>(TContext context)
    where TEntity : Entity<TEntityId>
    where TEntityId : notnull
    where TContext : DbContext
{
    protected readonly TContext Context = context;

    public IQueryable<TEntity> Query() => Context.Set<TEntity>();

    public async Task<TEntity?> GetByIdAsync(
        TEntityId id,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool enableTracking = true,
        CancellationToken ct = default)
    {
        IQueryable<TEntity> q = Query();
        if (!enableTracking) q = q.AsNoTracking();
        if (include is not null) q = include(q);
        return await q.FirstOrDefaultAsync(e => e.Id.Equals(id), ct);
    }

    public async Task<TEntity?> GetAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken ct = default)
    {
        IQueryable<TEntity> q = Query();
        if (!enableTracking) q = q.AsNoTracking();
        if (include is not null) q = include(q);
        if (withDeleted) q = q.IgnoreQueryFilters();
        return await q.FirstOrDefaultAsync(predicate, ct);
    }

    public async Task<Paginate<TEntity>> GetListAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        int index = 0,
        int size = 10,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken ct = default)
    {
        IQueryable<TEntity> q = Query();
        if (!enableTracking) q = q.AsNoTracking();
        if (include is not null) q = include(q);
        if (withDeleted) q = q.IgnoreQueryFilters();
        if (predicate is not null) q = q.Where(predicate);
        if (orderBy is not null) q = orderBy(q);
        return await q.ToPaginateAsync(index, size, ct);
    }

    public async Task<Paginate<TEntity>> GetListByDynamicAsync(
        DynamicQuery dynamic,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        int index = 0,
        int size = 10,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken ct = default)
    {
        IQueryable<TEntity> q = Query().ToDynamic(dynamic);
        if (!enableTracking) q = q.AsNoTracking();
        if (include is not null) q = include(q);
        if (withDeleted) q = q.IgnoreQueryFilters();
        if (predicate is not null) q = q.Where(predicate);
        return await q.ToPaginateAsync(index, size, ct);
    }

    public async Task<IEnumerable<TEntity>> GetListNoPaginateAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken ct = default)
    {
        IQueryable<TEntity> q = Query();
        if (!enableTracking) q = q.AsNoTracking();
        if (include is not null) q = include(q);
        if (withDeleted) q = q.IgnoreQueryFilters();
        if (predicate is not null) q = q.Where(predicate);
        if (orderBy is not null) q = orderBy(q);
        return await q.ToListAsync(ct);
    }

    public async Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        bool withDeleted = false,
        CancellationToken ct = default)
    {
        IQueryable<TEntity> q = Query();
        if (withDeleted) q = q.IgnoreQueryFilters();
        if (predicate is not null) q = q.Where(predicate);
        return await q.AnyAsync(ct);
    }
}
