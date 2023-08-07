using B1ServiceLayer.Interfaces;
using B1ServiceLayer.Models;
using System.Linq.Expressions;

namespace B1ServiceLayer.Extensions;

public static class SAPAsyncQueryableExtensions
{
    internal static IAsyncQueryProvider AsAsyncProvider(this IQueryable query)
    => (IAsyncQueryProvider)query.Provider;

    internal static async Task<TResult> ExecuteAsync<TResult>(this IQueryable query, CancellationToken cancellationToken = default)
        => await query.AsAsyncProvider().ExecuteAsync<TResult>(query.Expression, cancellationToken);

    public static async Task<List<TResult>> ToListAsync<TResult>(this IQueryable<TResult> query, CancellationToken cancellationToken = default)
        => (await query.ExecuteAsync<IEnumerable<TResult>>(cancellationToken)).ToList();

    public static async Task<TResult[]> ToArrayAsync<TResult>(this IQueryable<TResult> query, CancellationToken cancellationToken = default)
        => await query.ExecuteAsync<TResult[]>(cancellationToken);

    public static async Task<TResult> FirstAsync<TResult>(this IQueryable<TResult> query, CancellationToken cancellationToken = default)
        => (await query.Take(1).ExecuteAsync<IEnumerable<TResult>>(cancellationToken)).First();

    public static async Task<TResult> FirstAsync<TResult>(
        this IQueryable<TResult> query,
        Expression<Func<TResult, bool>> predicate,
        CancellationToken cancellationToken = default)
            => (await query.Take(1).Where(predicate).ExecuteAsync<IEnumerable<TResult>>(cancellationToken)).First();

    public static async Task<TResult?> FirstOrDefaultAsync<TResult>(this IQueryable<TResult> query, CancellationToken cancellationToken = default)
        => (await query.Take(1).ExecuteAsync<IEnumerable<TResult>>(cancellationToken)).FirstOrDefault();

    public static async Task<TResult?> FirstOrDefaultAsync<TResult>(
        this IQueryable<TResult> query,
        Expression<Func<TResult, bool>> predicate,
        CancellationToken cancellationToken = default)
            => (await query.Take(1).Where(predicate).ExecuteAsync<IEnumerable<TResult>>(cancellationToken)).FirstOrDefault();

    public static async Task<TResult> SingleAsync<TResult>(this IQueryable<TResult> query, CancellationToken cancellationToken = default)
       => (await query.Take(2).ExecuteAsync<IEnumerable<TResult>>(cancellationToken)).Single();

    public static async Task<TResult> SingleAsync<TResult>(
        this IQueryable<TResult> query,
        Expression<Func<TResult, bool>> predicate,
        CancellationToken cancellationToken = default)
            => (await query.Take(2).Where(predicate).ExecuteAsync<IEnumerable<TResult>>(cancellationToken)).Single();

    public static async Task<TResult?> SingleOrDefaultAsync<TResult>(this IQueryable<TResult> query, CancellationToken cancellationToken = default)
        => (await query.Take(2).ExecuteAsync<IEnumerable<TResult>>(cancellationToken)).SingleOrDefault();

    public static async Task<TResult?> SingleOrDefaultAsync<TResult>(
        this IQueryable<TResult> query,
        Expression<Func<TResult, bool>> predicate,
        CancellationToken cancellationToken = default)
            => (await query.Take(2).Where(predicate).ExecuteAsync<IEnumerable<TResult>>(cancellationToken)).SingleOrDefault();

    public static async Task<ValueCollection<TSource>> InlineCountAsync<TSource>(
        this IQueryable<TSource> query,
        CancellationToken cancellationToken = default)
            => await query.SetInlineCount().ExecuteAsync<ValueCollection<TSource>>(cancellationToken);

    /// <summary>
    /// Execute a queryable that will return a list of <typeparamref name="TResult"/>.
    /// </summary>
    /// <remarks>
    /// Queries with aggregation methods applied and cross join queries should be executed with this method.
    /// </remarks>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="query"></param>
    /// <returns></returns>
    public static async Task<List<TResult>> ExecuteQueryAsync<TSource, TResult>(this IQueryable<TSource> query)
        => await query.ExecuteAsync<List<TResult>>();

    /// <summary>
    /// Execute a queryable that will return a list of <typeparamref name="TResult"/>.
    /// </summary>
    /// <remarks>
    /// Queries with aggregation methods applied and cross join queries should be executed with this method.
    /// </remarks>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="query"></param>
    /// <returns></returns>
    public static async Task<List<TResult>> ExecuteQueryAsync<TSource, TResult>(this IQueryable<TSource> query, Expression<Func<TSource, TResult>> _)
        => await query.ExecuteQueryAsync<TSource, TResult>();
}
