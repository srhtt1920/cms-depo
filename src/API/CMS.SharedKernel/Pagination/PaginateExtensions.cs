using Microsoft.EntityFrameworkCore;

namespace CMS.SharedKernel.Pagination;

public static class PaginateExtensions
{
    public static async Task<Paginate<T>> ToPaginateAsync<T>(
        this IQueryable<T> source,
        int index,
        int size,
        CancellationToken cancellationToken = default)
    {
        var count = await source.CountAsync(cancellationToken);
        var items = await source
            .Skip(index * size)
            .Take(size)
            .ToListAsync(cancellationToken);

        return new Paginate<T>
        {
            Index = index,
            Size = size,
            Count = count,
            Items = items
        };
    }

    public static Paginate<T> ToPaginate<T>(
        this IQueryable<T> source,
        int index,
        int size)
    {
        var count = source.Count();
        var items = source.Skip(index * size).Take(size).ToList();

        return new Paginate<T>
        {
            Index = index,
            Size = size,
            Count = count,
            Items = items
        };
    }
}
