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

    public static IQueryable<TSource> NoPaginate<TSource>(this IQueryable<TSource> query)
        => query.Provider.CreateQuery<TSource>(Expression.Call(
            instance: null,
            method: NoPaginateInfo.MakeGenericMethod(typeof(TSource)),
            arguments: query.Expression));

    public static IQueryable<TSource> ApplySum<TSource, TResult>(this IQueryable<TSource> query, Expression<Func<TSource, TResult>> selector, string? resultFieldName = null)
        => query.ApplyAggregate(selector, SumInfo, resultFieldName);

    public static IQueryable<TSource> ApplyAverage<TSource, TResult>(this IQueryable<TSource> query, Expression<Func<TSource, TResult>> selector, string? resultFieldName = null)
        => query.ApplyAggregate(selector, AverageInfo, resultFieldName);

    public static IQueryable<TSource> ApplyMax<TSource, TResult>(this IQueryable<TSource> query, Expression<Func<TSource, TResult>> selector, string? resultFieldName = null)
        => query.ApplyAggregate(selector, MaxInfo, resultFieldName);

    public static IQueryable<TSource> ApplyMin<TSource, TResult>(this IQueryable<TSource> query, Expression<Func<TSource, TResult>> selector, string? resultFieldName = null)
        => query.ApplyAggregate(selector, MinInfo, resultFieldName);

    public static IQueryable<TSource> ApplyCountDistinct<TSource, TResult>(
        this IQueryable<TSource> query,
        Expression<Func<TSource, TResult>> selector,
        string? resultFieldName = null)
            => query.ApplyAggregate(selector, CountDistinctInfo, resultFieldName);

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

    public static List<TResult> ExecuteQuery<TSource, TResult>(this IQueryable<TSource> query)
        => query.Provider.Execute<List<TResult>>(query.Expression);

    public static List<TResult> ExecuteQuery<TSource, TResult>(this IQueryable<TSource> query, Expression<Func<TSource, TResult>> _)
        => query.ExecuteQuery<TSource, TResult>();

    public static IQueryable<TResult> CrossJoin<TSource, TTarget, TResult>(this IQueryable<TSource> query, Expression<Func<TSource, TTarget, TResult>> selector)
        => query.Provider.CreateQuery<TResult>(Expression.Call(
            instance: null,
            method: CrossJoinInfo.MakeGenericMethod(typeof(TSource), typeof(TTarget), typeof(TResult)),
            query.Expression, Expression.Quote(selector)));

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

    public static ValueCollection<TSource> InlineCount<TSource>(this IQueryable<TSource> query)
        => query.Provider.Execute<ValueCollection<TSource>>(query.SetInlineCount().Expression);
}
