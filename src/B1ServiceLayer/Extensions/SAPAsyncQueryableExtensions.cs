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

    /// <summary>
    /// Execute the query and creates a <see cref="List{T}"/>.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="query"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>A <see cref="List{T}"/> of <typeparamref name="TResult"/> values.</returns>
    public static async Task<List<TResult>> ToListAsync<TResult>(this IQueryable<TResult> query, CancellationToken cancellationToken = default)
        => (await query.ExecuteAsync<IEnumerable<TResult>>(cancellationToken)).ToList();

    /// <summary>
    /// Execute the query and creates an array.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="query"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>An array of <typeparamref name="TResult"/> values.</returns>
    public static async Task<TResult[]> ToArrayAsync<TResult>(this IQueryable<TResult> query, CancellationToken cancellationToken = default)
        => await query.ExecuteAsync<TResult[]>(cancellationToken);

    /// <inheritdoc cref="Queryable.First{TSource}(IQueryable{TSource})"/>
    public static async Task<TResult> FirstAsync<TResult>(this IQueryable<TResult> query, CancellationToken cancellationToken = default)
        => (await query.Take(1).ExecuteAsync<IEnumerable<TResult>>(cancellationToken)).First();

    /// <inheritdoc cref="Queryable.First{TSource}(IQueryable{TSource}, Expression{Func{TSource, bool}})"/>
    public static async Task<TResult> FirstAsync<TResult>(
        this IQueryable<TResult> query,
        Expression<Func<TResult, bool>> predicate,
        CancellationToken cancellationToken = default)
            => (await query.Take(1).Where(predicate).ExecuteAsync<IEnumerable<TResult>>(cancellationToken)).First();

    /// <inheritdoc cref="Queryable.FirstOrDefault{TSource}(IQueryable{TSource})" />
    public static async Task<TResult?> FirstOrDefaultAsync<TResult>(this IQueryable<TResult> query, CancellationToken cancellationToken = default)
        => (await query.Take(1).ExecuteAsync<IEnumerable<TResult>>(cancellationToken)).FirstOrDefault();

    /// <inheritdoc cref="Queryable.FirstOrDefault{TSource}(IQueryable{TSource}, Expression{Func{TSource, bool}})" />
    public static async Task<TResult?> FirstOrDefaultAsync<TResult>(
        this IQueryable<TResult> query,
        Expression<Func<TResult, bool>> predicate,
        CancellationToken cancellationToken = default)
            => (await query.Take(1).Where(predicate).ExecuteAsync<IEnumerable<TResult>>(cancellationToken)).FirstOrDefault();

    /// <inheritdoc cref="Queryable.Single{TSource}(IQueryable{TSource})" />
    public static async Task<TResult> SingleAsync<TResult>(this IQueryable<TResult> query, CancellationToken cancellationToken = default)
       => (await query.Take(2).ExecuteAsync<IEnumerable<TResult>>(cancellationToken)).Single();

    /// <inheritdoc cref="Queryable.Single{TSource}(IQueryable{TSource}, Expression{Func{TSource, bool}})" />
    public static async Task<TResult> SingleAsync<TResult>(
        this IQueryable<TResult> query,
        Expression<Func<TResult, bool>> predicate,
        CancellationToken cancellationToken = default)
            => (await query.Take(2).Where(predicate).ExecuteAsync<IEnumerable<TResult>>(cancellationToken)).Single();

    /// <inheritdoc cref="Queryable.SingleOrDefault{TSource}(IQueryable{TSource})" />
    public static async Task<TResult?> SingleOrDefaultAsync<TResult>(this IQueryable<TResult> query, CancellationToken cancellationToken = default)
        => (await query.Take(2).ExecuteAsync<IEnumerable<TResult>>(cancellationToken)).SingleOrDefault();

    /// <inheritdoc cref="Queryable.SingleOrDefault{TSource}(IQueryable{TSource}, Expression{Func{TSource, bool}})" />
    public static async Task<TResult?> SingleOrDefaultAsync<TResult>(
        this IQueryable<TResult> query,
        Expression<Func<TResult, bool>> predicate,
        CancellationToken cancellationToken = default)
            => (await query.Take(2).Where(predicate).ExecuteAsync<IEnumerable<TResult>>(cancellationToken)).SingleOrDefault();

    /// <summary>
    /// Execute an inline count that will return a record of values and the total count of values on database.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <param name="query"></param>
    /// <returns>A <see cref="ValueCollection{T}"/> instance with the values and total count of values.</returns>
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
