using B1ServiceLayer.Enums;
using B1ServiceLayer.Expressions;
using B1ServiceLayer.Extensions;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace B1ServiceLayer.Visitors;

public class SAPQueryExpressionVisitor: ExpressionVisitor
{
    [return: NotNullIfNotNull(nameof(node))]
    public override Expression? Visit(Expression? node)
        => base.Visit(node);

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        if (node.Method.DeclaringType != typeof(Queryable) && node.Method.DeclaringType != typeof(SAPQueryableExtensions))
            throw new NotImplementedException($"{node.Method.Name} method not implemented");

        var expression = Visit(node.Arguments[0]);

        if (expression is not SAPQueryExpression query)
            throw new InvalidOperationException($"Invalid expression {expression}");

        return node.Method.Name switch
        {
            nameof(Queryable.Where) => query.SetWhere(ExtractLambda(node.Arguments[1])),

            nameof(Queryable.Take) => query.SetTop(ExtractConstant(node.Arguments[1])),

            nameof(Queryable.Skip) => query.SetSkip(ExtractConstant(node.Arguments[1])),

            nameof(Queryable.Select) => query.SetSelect(ExtractLambda(node.Arguments[1])),

            nameof(SAPQueryableExtensions.NoPaginate) => query.IgnorePagination(),

            nameof(SAPQueryableExtensions.ApplySum) => query.AddAggregate(CreateAggregate(node, AggregateOperation.Sum)),

            nameof(SAPQueryableExtensions.ApplyAverage) => query.AddAggregate(CreateAggregate(node, AggregateOperation.Average)),

            nameof(SAPQueryableExtensions.ApplyMax) => query.AddAggregate(CreateAggregate(node, AggregateOperation.Max)),

            nameof(SAPQueryableExtensions.ApplyMin) => query.AddAggregate(CreateAggregate(node, AggregateOperation.Min)),

            nameof(SAPQueryableExtensions.ApplyCountDistinct) => query.AddAggregate(CreateAggregate(node, AggregateOperation.CountDistinct)),

            nameof(SAPQueryableExtensions.ApplyCount) => query.AddAggregate(CreateAggregate(node, AggregateOperation.Count)),

            nameof(Queryable.GroupBy) => query.SetGroupBy(ExtractMembers(ExtractLambda(node.Arguments[1]))),

            nameof(SAPQueryableExtensions.InlineCount) => query.HasInlineCount(),

            var name when
                name.In(nameof(Queryable.OrderBy), nameof(Queryable.ThenBy))
                    => query.AddOrderBy(ExtractMember(node.Arguments[1])),

            var name when
                name.In(nameof(Queryable.OrderByDescending), nameof(Queryable.ThenByDescending))
                    => query.AddOrderBy(ExtractMember(node.Arguments[1]), "desc"),

            var name when
                name == nameof(Queryable.Count) && node.Arguments.Count > 1 => query.SetWhere(ExtractLambda(node.Arguments[1])).Count(),

            nameof(Queryable.Count) => query.Count(),

            var name when
                name == nameof(SAPQueryableExtensions.CrossJoin) && node.Arguments.Count > 2
                    => SetCrossJoin(query.SetWhere(ExtractLambda(node.Arguments[2])), node, ExtractLambda(node.Arguments[1])),

            nameof(SAPQueryableExtensions.CrossJoin) => SetCrossJoin(query, node, ExtractLambda(node.Arguments[1])),

            var name when
                name == nameof(Queryable.First) && node.Arguments.Count > 1
                    => query.SetTop(1).SetWhere(ExtractLambda(node.Arguments[1])).SetQueryableMethod(_firstMethod),
            nameof(Queryable.First) => query.SetTop(1).SetQueryableMethod(_firstMethod),

            var name when
                name == nameof(Queryable.FirstOrDefault) && node.Arguments.Count > 1
                    => query.SetTop(1).SetWhere(ExtractLambda(node.Arguments[1])).SetQueryableMethod(_firstOrDefaultMethod),
            nameof(Queryable.FirstOrDefault) => query.SetTop(1).SetQueryableMethod(_firstOrDefaultMethod),

            var name when
                name == nameof(Queryable.Single) && node.Arguments.Count > 1
                    => query.SetTop(2).SetWhere(ExtractLambda(node.Arguments[1])).SetQueryableMethod(_singleMethod),
            nameof(Queryable.Single) => query.SetTop(2).SetQueryableMethod(_singleMethod),

            var name when
                name == nameof(Queryable.SingleOrDefault) && node.Arguments.Count > 1
                    => query.SetTop(2).SetWhere(ExtractLambda(node.Arguments[1])).SetQueryableMethod(_singleOrDefaultMethod),
            nameof(Queryable.SingleOrDefault) => query.SetTop(2).SetQueryableMethod(_singleOrDefaultMethod),

            _ => query,
        };
    }

    protected override Expression VisitExtension(Expression node)
    {
        if (node is SAPRootExpression root)
            return SAPQueryExpression.From(root);

        return node;
    }

    private static readonly MethodInfo _firstMethod = typeof(Enumerable).GetMethods(BindingFlags.Public | BindingFlags.Static)
        .First(e => e.Name == nameof(Enumerable.First));

    private static readonly MethodInfo _firstOrDefaultMethod = typeof(Enumerable).GetMethods(BindingFlags.Public | BindingFlags.Static)
        .First(e => e.Name == nameof(Enumerable.FirstOrDefault));

    private static readonly MethodInfo _singleMethod = typeof(Enumerable).GetMethods(BindingFlags.Public | BindingFlags.Static)
        .First(e => e.Name == nameof(Enumerable.Single));

    private static readonly MethodInfo _singleOrDefaultMethod = typeof(Enumerable).GetMethods(BindingFlags.Public | BindingFlags.Static)
        .First(e => e.Name == nameof(Enumerable.SingleOrDefault));

    private static SAPAggregateExpression CreateAggregate(MethodCallExpression node, AggregateOperation operation)
        => new(node.Method.GetGenericArguments()[0], ExtractMember(node.Arguments[1]), operation, ExtractConstant(node.Arguments[2]));

    private static LambdaExpression ExtractLambda(Expression expression)
    {
        if (expression.NodeType == ExpressionType.Quote && expression is UnaryExpression unary)
        {
            return (LambdaExpression)unary.Operand;
        }

        return (LambdaExpression)expression;
    }

    private static MemberExpression ExtractMember(Expression expression)
        => (MemberExpression)ExtractLambda(expression).Body;

    private static ConstantExpression ExtractConstant(Expression expression)
    {
        if (expression is UnaryExpression unary)
            return ExtractConstant(unary.Operand);

        return (ConstantExpression)expression;
    }

    private static IEnumerable<MemberExpression> ExtractMembers(LambdaExpression expression)
    {
        if (expression.Body is MemberExpression memberExpression)
        {
            return new MemberExpression[] { memberExpression };
        }

        if (expression.Body is NewExpression newExpression && newExpression.Members is not null)
        {
            return newExpression.Members.Select(e => Expression.MakeMemberAccess(newExpression, e));
        }

        return Array.Empty<MemberExpression>();
    }

    private static SAPQueryExpression SetCrossJoin(SAPQueryExpression query, MethodCallExpression method, LambdaExpression expression)
    {
        var t1Type = method.Method.GetGenericArguments()[1];

        query.SetExpand(expression).AddResource(t1Type);
        return query;
    }
}
