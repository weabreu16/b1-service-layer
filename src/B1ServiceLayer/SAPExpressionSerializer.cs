using System.Linq.Expressions;
using B1ServiceLayer.Extensions;

namespace B1ServiceLayer;

public class SAPExpressionSerializer
{
    public static string? Serialize(Expression expression)
    {
        if (expression is UnaryExpression unaryExpression)
            return GetUnaryExpression(unaryExpression);

        if (expression is BinaryExpression binaryExpression)
            return GetBinaryExpression(binaryExpression);

        if (expression is MethodCallExpression callExpression)
            return GetCallExpression(callExpression);

        return null;
    }

    public static string GetUnaryExpression(UnaryExpression expression)
    {
        Expression operand = expression.Operand;

        return expression.NodeType switch
        {
            ExpressionType.Convert => ExtractValue(operand),
            ExpressionType.Not => $"not {ExtractValue(operand)}",
            _ => throw new NotSupportedException()
        };
    }

    public static string GetBinaryExpression(BinaryExpression expression)
    {
        Expression left = expression.Left;
        Expression right = expression.Right;

        string operate(string operation) => GetExpressionQuery(left, right, operation);

        return expression.NodeType switch
        {
            ExpressionType.AndAlso => operate("and"),
            ExpressionType.Equal => operate("eq"),
            ExpressionType.GreaterThan => operate("gt"),
            ExpressionType.GreaterThanOrEqual => operate("ge"),
            ExpressionType.LessThan => operate("lt"),
            ExpressionType.LessThanOrEqual => operate("le"),
            ExpressionType.NotEqual => operate("ne"),
            ExpressionType.OrElse => operate("or"),
            _ => throw new NotSupportedException(),
        };
    }

    public static string GetCallExpression(MethodCallExpression expression)
    {
        if (!expression.Method.Name.In("Contains", "SubstringOf", "StartsWith", "EndsWith"))
            throw new NotSupportedException($"Method {expression.Method.Name} not supported");

        if (expression.Object is null)
            return GetCallExpression(expression.Method.Name, expression.Arguments[0], expression.Arguments[1]);

        return GetCallExpression(expression.Method.Name, expression.Object, expression.Arguments[0]);
    }

    public static string GetCallExpression(string methodName, Expression arg1, Expression arg2)
        => GetCallExpression(methodName, ExtractValue(arg1), ExtractValue(arg2));

    public static string GetCallExpression(string methodName, params string[] args)
        => $"{methodName.ToLower()}({string.Join(',', args)})";

    public static string ExtractValue(Expression expression)
    {
        if (expression is BinaryExpression || expression is UnaryExpression || expression is MethodCallExpression)
            return Serialize(expression) ?? "null";

        if (expression is MemberExpression member)
            return ExtractValue(member);

        return ExtractConstantValue(expression);
    }

    public static string ExtractValue(MemberExpression member)
    {
        if (member.Expression?.NodeType == ExpressionType.Constant)
            return GetValueAsQueryFormatted(GetValue(member));

        return member.Member.Name;
    }

    public static string ExtractConstantValue(Expression expression)
        => GetValueAsQueryFormatted(((ConstantExpression)expression).Value);

    public static object? GetValue(MemberExpression member)
    {
        var objectMember = Expression.Convert(member, typeof(object));

        var getterLambda = Expression.Lambda<Func<object?>>(objectMember);

        var getter = getterLambda.Compile();

        return getter();
    }

    public static string GetValueAsQueryFormatted(object? value)
    {
        if (value is null)
            return "null";

        Type type = value.GetType();

        if ((type.IsClass && type == typeof(string)) || type == typeof(DateTime) || type.IsEnum)
            return $"'{value}'";

        if (type.IsClass && !type.IsNumeric())
            return GetObjectExpression(value);

        return $"{value}";
    }

    private static string GetObjectExpression(object obj)
    {
        Dictionary<string, string> result = new();

        var props = obj.GetType().GetProperties();

        foreach (var prop in props)
        {
            var value = prop.GetValue(obj, null);

            result.Add(prop.Name, GetValueAsQueryFormatted(value));
        }

        return string.Join(", ", result.Select(e => $"{e.Key}={e.Value}"));
    }

    private static string GetExpressionQuery(Expression left, Expression right, string operation)
        => $"({ExtractValue(left)} {operation} {ExtractValue(right)})";

    public static Func<TSource, TSource> DeserializeSelect<TSource, TResult>(string select)
    {
        string[] selectedProps = select.ReplaceWhitespace("").Split(",");
        var sourceProps = typeof(TSource).GetProperties().ToDictionary(e => e.Name);

        if (selectedProps.Length == 1)
            return (source) => (TSource)sourceProps[selectedProps[0]].GetValue(source)!;

        return (source) => (TSource)Activator.CreateInstance(typeof(TSource))!;
    }
}
