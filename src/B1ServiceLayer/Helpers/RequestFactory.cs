using B1ServiceLayer.Expressions;
using B1ServiceLayer.Visitors;
using RestSharp;
using System.Linq.Expressions;

namespace B1ServiceLayer.Helpers;

public static class RequestFactory
{
    public static RestRequest Create(SAPQueryExpression expression)
    {
        if (expression.Resources.Count > 1) return CreateCrossJoined(expression);

        return CreateSingle(expression);
    }

    public static RestRequest CreateSingle(SAPQueryExpression expression)
    {
        string resource = expression.Resources.First().Value;

        var request = new RestRequest(resource);

        if (expression.IsCounting)
            request.Resource += "/$count";

        if (!expression.IsPaginated)
            request.AddHeader("Prefer", "odata.maxpagesize=0");

        var visitor = new SAPWhereExpressionVisitor();

        AddQueryParameters(expression, request, visitor);

        request.AddQueryParameter("$select",
            expression.Select is not null
                ? TranslateSelect(expression.Select)
                : GetSelect(expression.ElementType));

        return request;
    }

    public static RestRequest CreateCrossJoined(SAPQueryExpression expression)
    {
        string resource = $"$crossjoin({string.Join(',', expression.Resources.Values)})";

        var request = new RestRequest(resource);

        if (!expression.IsPaginated)
            request.AddHeader("Prefer", "odata.maxpagesize=0");

        SAPWhereExpressionVisitor visitor = new SAPJoinWhereExpressionVisitor(expression.Resources);

        AddQueryParameters(expression, request, visitor);

        if (expression.Expand is not null)
            request.AddQueryParameter("$expand", TranslateExpand(expression.Resources, expression.Expand), false);

        return request;
    }

    private static void AddQueryParameters(SAPQueryExpression expression, RestRequest request, SAPWhereExpressionVisitor visitor)
    {
        visitor.Visit(expression.Where);

        if (!string.IsNullOrEmpty(visitor.Query))
            request.AddQueryParameter("$filter", visitor.Query);

        if (expression.Top is not null)
            request.AddQueryParameter("$top", expression.Top.Value);

        if (expression.Skip is not null)
            request.AddQueryParameter("$skip", expression.Skip.Value);

        if (expression.Orders.Count != 0)
            request.AddQueryParameter("$orderby", string.Join(',', expression.Orders));

        if (expression.Aggregates.Count != 0)
            request.AddQueryParameter("$apply", TranslateApply(expression), false);

        if (expression.InlineCount)
            request.AddQueryParameter("$inlinecount", "allpages");
    }

    private static string TranslateSelect(LambdaExpression expression)
    {
        if (expression.Body is MemberExpression memberExpression)
            return memberExpression.Member.Name;

        if (expression.Body is NewExpression newExpression && newExpression.Members is not null)
            return string.Join(", ", newExpression.Members.Select(e => e.Name));

        return expression.ToString();
    }

    private static string GetSelect(Type type)
        => string.Join(", ", type.GetProperties().Select(e => e.Name));

    private static string? TranslateApply(SAPQueryExpression query)
    {
        string? aggregates = TranslateAggregates(query.Aggregates);
        string? groupby = TranslateGroupBy(query.GroupBy);

        if (groupby is null)
            return aggregates;

        if (aggregates is null)
            return groupby;

        return $"{aggregates}/{groupby}";
    }

    private static string? TranslateAggregates(List<SAPAggregateExpression> aggregates)
        => aggregates.Count == 0 ? null : $"aggregate({string.Join(',', aggregates)})";

    private static string? TranslateGroupBy(IEnumerable<MemberExpression> members)
        => !members.Any() ? null : $"groupby(({string.Join(',', members.Select(e => e.Member.Name))}))";

    private static string TranslateExpand(Dictionary<Type, string> resources, LambdaExpression expandExpression)
    {
        if (expandExpression.Body is not NewExpression newExpression || newExpression.Members is null)
            throw new InvalidOperationException("Cross Join selector should return a new expression");

        List<string> expandStatements = new();

        var parameters = expandExpression.Parameters;

        foreach (var parameter in parameters)
            expandStatements.Add($"{resources[parameter.Type]}($select={string.Join(',',
                newExpression.Arguments
                    .Cast<MemberExpression>()
                    .Where(e => e.Member.DeclaringType == parameter.Type)
                    .Select(e => e.Member.Name)
                    .ToList())})");

        return string.Join(',', expandStatements);
    }
}
