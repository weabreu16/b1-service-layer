using B1ServiceLayer.Models;
using System.Linq.Expressions;
using System.Reflection;

namespace B1ServiceLayer.Extensions;

public static class SAPQueryableExtensions
{
    private static readonly Type Type = typeof(SAPQueryableExtensions);

    internal static readonly MethodInfo NoPaginateInfo = Type.GetMethod(nameof(NoPaginate))!;
    internal static readonly MethodInfo InlineCountInfo = Type.GetMethod(nameof(InlineCount))!;

    internal static readonly MethodInfo SumInfo = Type.GetMethod(nameof(ApplySum))!;
    internal static readonly MethodInfo AverageInfo = Type.GetMethod(nameof(ApplyAverage))!;
    internal static readonly MethodInfo MaxInfo = Type.GetMethod(nameof(ApplyMax))!;
    internal static readonly MethodInfo MinInfo = Type.GetMethod(nameof(ApplyMin))!;
    internal static readonly MethodInfo CountDistinctInfo = Type.GetMethod(nameof(ApplyCountDistinct))!;
    internal static readonly MethodInfo CountInfo = Type.GetMethod(nameof(ApplyCount))!;

    internal static readonly MethodInfo CrossJoinInfo = Type.GetMethods(BindingFlags.Public | BindingFlags.Static)
        .Where(e => e.Name == nameof(CrossJoin) && e.GetParameters().Length == 2).First();

    internal static readonly MethodInfo CrossJoinWithWhereInfo = Type.GetMethods(BindingFlags.Public | BindingFlags.Static)
        .Where(e => e.Name == nameof(CrossJoin) && e.GetParameters().Length == 3).First();

    /// <summary>
    /// Disable this query's pagination.
    /// </summary>
    /// <remarks>
    /// By default, SAP B1 Service Layer apply a page size of 20. 
    /// You can customize this by changing the PageSize in SAP B1 Service Layer configuration file conf/b1s.conf.
    /// </remarks>
    /// <typeparam name="TSource"></typeparam>
    /// <param name="query"></param>
    /// <returns></returns>
    public static IQueryable<TSource> NoPaginate<TSource>(this IQueryable<TSource> query)
        => query.Provider.CreateQuery<TSource>(Expression.Call(
            instance: null,
            method: NoPaginateInfo.MakeGenericMethod(typeof(TSource)),
            arguments: query.Expression));

    /// <summary>
    /// Execute a sum aggregation method to get the sum of target property values.
    /// </summary>
    /// <remarks>
    /// The returned <see cref="IQueryable{TSource}"/> should be executed with <see cref="ExecuteQuery{TSource, TResult}(IQueryable{TSource})"/> method.
    /// </remarks>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="query"></param>
    /// <param name="selector">Property to which the sum will be applied.</param>
    /// <param name="resultFieldName">The result field name that will have the sum applied.</param>
    /// <returns></returns>
    public static IQueryable<TSource> ApplySum<TSource, TResult>(this IQueryable<TSource> query, Expression<Func<TSource, TResult>> selector, string? resultFieldName = null)
        => query.ApplyAggregate(selector, SumInfo, resultFieldName);

    /// <summary>
    /// Execute an average aggregation method to get the average value of target property.
    /// </summary>
    /// <remarks>
    /// The returned <see cref="IQueryable{TSource}"/> should be executed with <see cref="ExecuteQuery{TSource, TResult}(IQueryable{TSource})"/> method.
    /// </remarks>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="query"></param>
    /// <param name="selector">Property to which the average will be applied.</param>
    /// <param name="resultFieldName">The result field name that will have the average applied.</param>
    /// <returns></returns>
    public static IQueryable<TSource> ApplyAverage<TSource, TResult>(this IQueryable<TSource> query, Expression<Func<TSource, TResult>> selector, string? resultFieldName = null)
        => query.ApplyAggregate(selector, AverageInfo, resultFieldName);

    /// <summary>
    /// Execute a max aggregation method to get the maximum value of target property.
    /// </summary>
    /// <remarks>
    /// The returned <see cref="IQueryable{TSource}"/> should be executed with <see cref="ExecuteQuery{TSource, TResult}(IQueryable{TSource})"/> method.
    /// </remarks>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="query"></param>
    /// <param name="selector">Property to which the max aggregation will be applied.</param>
    /// <param name="resultFieldName">The result field name that will have the maximum value.</param>
    /// <returns></returns>
    public static IQueryable<TSource> ApplyMax<TSource, TResult>(this IQueryable<TSource> query, Expression<Func<TSource, TResult>> selector, string? resultFieldName = null)
        => query.ApplyAggregate(selector, MaxInfo, resultFieldName);

    /// <summary>
    /// Execute a min aggregation method to get the minimum value of target property.
    /// </summary>
    /// <remarks>
    /// The returned <see cref="IQueryable{TSource}"/> should be executed with <see cref="ExecuteQuery{TSource, TResult}(IQueryable{TSource})"/> method.
    /// </remarks>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="query"></param>
    /// <param name="selector">Property to which the mim aggregation will be applied.</param>
    /// <param name="resultFieldName">The result field name that will have the minimum value.</param>
    /// <returns></returns>
    public static IQueryable<TSource> ApplyMin<TSource, TResult>(this IQueryable<TSource> query, Expression<Func<TSource, TResult>> selector, string? resultFieldName = null)
        => query.ApplyAggregate(selector, MinInfo, resultFieldName);

    /// <summary>
    /// Execute a count distinct aggregation method to get the count of target property distinct values.
    /// </summary>
    /// <remarks>
    /// The returned <see cref="IQueryable{TSource}"/> should be executed with <see cref="ExecuteQuery{TSource, TResult}(IQueryable{TSource})"/> method.
    /// </remarks>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="query"></param>
    /// <param name="selector">Property to which the count distinct aggregation will be applied.</param>
    /// <param name="resultFieldName">The result field name that will have the count.</param>
    /// <returns></returns>
    public static IQueryable<TSource> ApplyCountDistinct<TSource, TResult>(
        this IQueryable<TSource> query,
        Expression<Func<TSource, TResult>> selector,
        string? resultFieldName = null)
            => query.ApplyAggregate(selector, CountDistinctInfo, resultFieldName);

    /// <summary>
    /// Execute a count aggregation method to get the count of target property values.
    /// </summary>
    /// <remarks>
    /// The returned <see cref="IQueryable{TSource}"/> should be executed with <see cref="ExecuteQuery{TSource, TResult}(IQueryable{TSource})"/> method.
    /// </remarks>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="query"></param>
    /// <param name="selector">Property to which the count aggregation will be applied.</param>
    /// <param name="resultFieldName">The result field name that will have the count.</param>
    /// <returns></returns>
    public static IQueryable<TSource> ApplyCount<TSource, TResult>(this IQueryable<TSource> query, Expression<Func<TSource, TResult>> selector, string? resultFieldName = null)
        => query.ApplyAggregate(selector, CountInfo, resultFieldName);

    private static IQueryable<TSource> ApplyAggregate<TSource, TResult>(
        this IQueryable<TSource> query,
        Expression<Func<TSource, TResult>> selector,
        MethodInfo method,
        string? resultFieldName = null)
            => query.Provider.CreateQuery<TSource>(Expression.Call(
                instance: null,
                method: method.MakeGenericMethod(typeof(TSource), typeof(TResult)),
                query.Expression, Expression.Quote(selector), Expression.Constant(resultFieldName, typeof(string))));

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
    public static List<TResult> ExecuteQuery<TSource, TResult>(this IQueryable<TSource> query)
        => query.Provider.Execute<List<TResult>>(query.Expression);

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
    public static List<TResult> ExecuteQuery<TSource, TResult>(this IQueryable<TSource> query, Expression<Func<TSource, TResult>> _)
        => query.ExecuteQuery<TSource, TResult>();

    /// <summary>
    /// Create a cross join query and select the properties of that should be returned of each object.
    /// </summary>
    /// <remarks>
    /// The returned <see cref="IQueryable{TSource}"/> should be executed with <see cref="ExecuteQuery{TSource, TResult}(IQueryable{TSource})"/> method.
    /// </remarks>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TTarget"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="query"></param>
    /// <param name="selector"></param>
    /// <returns></returns>
    public static IQueryable<TResult> CrossJoin<TSource, TTarget, TResult>(this IQueryable<TSource> query, Expression<Func<TSource, TTarget, TResult>> selector)
        => query.Provider.CreateQuery<TResult>(Expression.Call(
            instance: null,
            method: CrossJoinInfo.MakeGenericMethod(typeof(TSource), typeof(TTarget), typeof(TResult)),
            query.Expression, Expression.Quote(selector)));

    /// <summary>
    /// Create a cross join query and select the properties of that should be returned of each object.
    /// </summary>
    /// <remarks>
    /// The returned <see cref="IQueryable{TSource}"/> should be executed with <see cref="ExecuteQuery{TSource, TResult}(IQueryable{TSource})"/> method.
    /// </remarks>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TTarget"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="query"></param>
    /// <param name="selector"></param>
    /// <param name="predicate">The condition to which the cross join should be accord.</param>
    /// <returns></returns>
    public static IQueryable<TResult> CrossJoin<TSource, TTarget, TResult>(
        this IQueryable<TSource> query,
        Expression<Func<TSource, TTarget, TResult>> selector,
        Expression<Func<TSource, TTarget, bool>> predicate)
        => query.Provider.CreateQuery<TResult>(Expression.Call(
            instance: null,
            method: CrossJoinWithWhereInfo.MakeGenericMethod(typeof(TSource), typeof(TTarget), typeof(TResult)),
            query.Expression, Expression.Quote(selector), Expression.Quote(predicate)));

    internal static IQueryable<TSource> SetInlineCount<TSource>(this IQueryable<TSource> query)
        => query.Provider.CreateQuery<TSource>(Expression.Call(
            instance: null,
            method: InlineCountInfo.MakeGenericMethod(typeof(TSource)),
            query.Expression));

    /// <summary>
    /// Execute an inline count that will return a record of values and the total count of values on database.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <param name="query"></param>
    /// <returns>A <see cref="ValueCollection{T}"/> instance with the values and total count of values.</returns>
    public static ValueCollection<TSource> InlineCount<TSource>(this IQueryable<TSource> query)
        => query.Provider.Execute<ValueCollection<TSource>>(query.SetInlineCount().Expression);
}
