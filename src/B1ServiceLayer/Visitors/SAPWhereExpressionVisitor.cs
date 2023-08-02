using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text;

namespace B1ServiceLayer.Visitors;

public class SAPWhereExpressionVisitor : ExpressionVisitor
{
    protected readonly StringBuilder _query;

    internal static readonly HashSet<string> _supportedMethodNames
        = new() { nameof(string.Contains), nameof(SAPFunctions.SubstringOf), nameof(string.StartsWith), nameof(string.EndsWith) };

    internal static readonly Dictionary<ExpressionType, string> _binaryExpressionTypes = new()
    {
        { ExpressionType.Equal, " eq " },
        { ExpressionType.NotEqual, " ne " },
        { ExpressionType.AndAlso, " and " },
        { ExpressionType.OrElse, " or " },
        { ExpressionType.GreaterThan, " gt " },
        { ExpressionType.GreaterThanOrEqual, " ge " },
        { ExpressionType.LessThan, " lt " },
        { ExpressionType.LessThanOrEqual, " le " }
    };

    public string Query => _query.ToString();

    public SAPWhereExpressionVisitor()
    {
        _query = new();
    }

    [return: NotNullIfNotNull(nameof(node))]
    public override Expression? Visit(Expression? node)
    {
        if (node is null) return null;

        if (node is MemberExpression member)
            VisitMember(member);

        else if (node is ConstantExpression constant)
            VisitConstant(constant);

        else if (node is UnaryExpression unary)
            VisitUnary(unary);

        else if (node is MethodCallExpression call)
            VisitMethodCall(call);

        else if (node.NodeType == ExpressionType.Lambda)
            Visit(((LambdaExpression)node).Body);

        else if (_binaryExpressionTypes.ContainsKey(node.NodeType))
            VisitBinary((BinaryExpression)node);

        return node;
    }

    protected override Expression VisitConstant(ConstantExpression node)
    {
        Append(SAPExpressionSerializer.GetValueAsQueryFormatted(node.Value));

        return node;
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        Append(node.Member.Name);

        return node;
    }

    protected override Expression VisitUnary(UnaryExpression node)
    {
        if (node.NodeType == ExpressionType.Not)
        {
            Append("not ");
            Visit(node.Operand);
        }
        else Visit(node.Operand);

        return node;
    }

    protected override Expression VisitBinary(BinaryExpression node)
    {
        Append("(");
        Visit(node.Left);

        if (_binaryExpressionTypes.TryGetValue(node.NodeType, out string? operation))
        {
            Append(operation);
        }

        Visit(node.Right);
        Append(")");

        return node;
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        if (!_supportedMethodNames.Contains(node.Method.Name)) return node;

        Append($"{node.Method.Name.ToLowerInvariant()}(");

        if (node.Object is not null)
        {
            Visit(node.Object);
            Append(", ");
            Visit(node.Arguments[0]);
        }
        else
        {
            Visit(node.Arguments[0]);
            Append(", ");
            Visit(node.Arguments[1]);
        }

        Append(")");

        return node;
    }

    protected override Expression VisitLambda<T>(Expression<T> node)
    {
        return Visit(node.Body);
    }

    protected void Append(string text) => _query.Append(text);
}
