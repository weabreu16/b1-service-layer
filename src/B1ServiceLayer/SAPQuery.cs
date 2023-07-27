using RestSharp;
using System.Linq.Expressions;
using B1ServiceLayer.Models;
using B1ServiceLayer.Enums;
using B1ServiceLayer.Extensions;

namespace B1ServiceLayer;

public class SAPQuery<TSAPObject>
{
    public B1Service Provider { get; }
    public string ResourceName { get; }

    private bool _isPaginated = true;
    private int? _top;
    private int? _skip;
    private string? _select;
    private string? _filter;
    private string? _orderBy;
    private List<string>? _aggregateFields;
    private string? _groupBy;

    private List<string> AggregateFields => _aggregateFields ??= new();

    internal SAPQuery(B1Service sapService, string resourceName)
    {
        Provider = sapService;
        ResourceName = resourceName;
    }

    private SAPQuery(B1Service sapService, string resourceName, bool isPaginated, int? top, int? skip, string? select, string? filter, string? orderBy)
    {
        Provider = sapService;
        ResourceName = resourceName;
        _isPaginated = isPaginated;
        _top = top;
        _skip = skip;
        _select = select;
        _filter = filter;
        _orderBy = orderBy;
    }

    public SAPQuery<TSAPObject> Where(string predicate)
    {
        _filter = predicate;
        return this;
    }

    public SAPQuery<TSAPObject> Where(Expression<Func<TSAPObject, bool>> predicate)
    {
        _filter = SAPExpressionSerializer.Serialize(predicate.Body);
        return this;
    }

    public SAPQuery<TResult> Select<TResult>(Expression<Func<TSAPObject, TResult>> predicate)
    {
        if (predicate.Body is MemberExpression memberExpression)
            _select = memberExpression.Member.Name;

        if (predicate.Body is NewExpression newExpression && newExpression.Members is not null)
            _select = string.Join(", ", newExpression.Members.Select(e => e.Name));

        return new SAPQuery<TResult>(Provider, ResourceName, _isPaginated, _top, _skip, _select, _filter, _orderBy);
    }

    public SAPQuery<TSAPObject> Top(int qty)
    {
        _top = qty;
        return this;
    }

    public SAPQuery<TSAPObject> Skip(int qty)
    {
        _skip = qty;
        return this;
    }

    public SAPQuery<TSAPObject> NoPaginate()
    {
        _isPaginated = false;
        return this;
    }

    public SAPQuery<TSAPObject> OrderBy(string orderBy)
    {
        _orderBy = orderBy;

        return this;
    }

    public SAPQuery<TSAPObject> OrderBy(Expression<Func<TSAPObject, object?>> predicate)
    {
        _orderBy = $"{ExtractProperty(predicate.Body)} asc";

        return this;
    }

    public SAPQuery<TSAPObject> OrderByDesc(Expression<Func<TSAPObject, object?>> predicate)
    {
        _orderBy = $"{ExtractProperty(predicate.Body)} desc";

        return this;
    }

    public SAPQuery<TSAPObject> ThenOrderBy(Expression<Func<TSAPObject, object?>> predicate)
    {
        if (string.IsNullOrEmpty(_orderBy))
            throw new InvalidOperationException($"Can't add secondary order by statement without the main statement, use {nameof(OrderBy)} instead");

        _orderBy += $", {ExtractProperty(predicate.Body)} asc";

        return this;
    }

    public SAPQuery<TSAPObject> ThenOrderByDesc(Expression<Func<TSAPObject, object?>> predicate)
    {
        if (string.IsNullOrEmpty(_orderBy))
            throw new InvalidOperationException($"Can't add secondary order by statement without the main statement, use {nameof(OrderByDesc)} instead");

        _orderBy += $", {ExtractProperty(predicate.Body)} desc";

        return this;
    }

    public ICollection<TSAPObject> Get()
        => GetAsync().GetAwaiter().GetResult();

    public async Task<ICollection<TSAPObject>> GetAsync()
    {
        var response = await Provider.ExecuteAsync<SapResponse<TSAPObject>>(BuildRequest());

        if (response is null)
            return Array.Empty<TSAPObject>();

        return response.Value;
    }

    public ValueCollection<TSAPObject>? GetWithCount()
        => GetWithCountAsync().GetAwaiter().GetResult();

    public async Task<ValueCollection<TSAPObject>?> GetWithCountAsync()
        => await Provider.ExecuteAsync<ValueCollection<TSAPObject>>(BuildRequest(includeInlineCount: true));

    public ICollection<TResult> Apply<TResult>(string applyStatement)
        => ApplyAsync<TResult>(applyStatement).GetAwaiter().GetResult();

    public async Task<ICollection<TResult>> ApplyAsync<TResult>(string applyStatement, CancellationToken cancellationToken = default)
    {
        var request = BuildRequest();

        request.AddQueryParameter("$apply", applyStatement, false);

        var response = await Provider.ExecuteAsync<SapResponse<TResult>>(request, cancellationToken);
        return response!.Value;
    }

    public SAPQuery<TSAPObject> Sum<TField>(string resultFieldName, Expression<Func<TSAPObject, TField>> selector)
        => Aggregate(selector, AggregateOperation.Sum, resultFieldName);

    public SAPQuery<TSAPObject> Sum<TField>(Expression<Func<TSAPObject, TField>> selector)
        => Aggregate(selector, AggregateOperation.Sum);

    public SAPQuery<TSAPObject> Average<TField>(string resultFieldName, Expression<Func<TSAPObject, TField>> selector)
        => Aggregate(selector, AggregateOperation.Average, resultFieldName);

    public SAPQuery<TSAPObject> Average<TField>(Expression<Func<TSAPObject, TField>> selector)
        => Aggregate(selector, AggregateOperation.Average);

    public SAPQuery<TSAPObject> Max<TField>(string resultFieldName, Expression<Func<TSAPObject, TField>> selector)
        => Aggregate(selector, AggregateOperation.Max, resultFieldName);

    public SAPQuery<TSAPObject> Max<TField>(Expression<Func<TSAPObject, TField>> selector)
        => Aggregate(selector, AggregateOperation.Max);

    public SAPQuery<TSAPObject> Min<TField>(string resultFieldName, Expression<Func<TSAPObject, TField>> selector)
        => Aggregate(selector, AggregateOperation.Min, resultFieldName);

    public SAPQuery<TSAPObject> Min<TField>(Expression<Func<TSAPObject, TField>> selector)
        => Aggregate(selector, AggregateOperation.Min);

    public SAPQuery<TSAPObject> CountDistinct<TField>(string resultFieldName, Expression<Func<TSAPObject, TField>> selector)
        => Aggregate(selector, AggregateOperation.CountDistinct, resultFieldName);

    public SAPQuery<TSAPObject> CountDistinct<TField>(Expression<Func<TSAPObject, TField>> selector)
        => Aggregate(selector, AggregateOperation.CountDistinct);

    public SAPQuery<TSAPObject> Count<TField>(string resultFieldName, Expression<Func<TSAPObject, TField>> selector)
        => Aggregate(selector, AggregateOperation.Count, resultFieldName);

    public SAPQuery<TSAPObject> Count<TField>(Expression<Func<TSAPObject, TField>> selector)
        => Aggregate(selector, AggregateOperation.Count);

    public SAPQuery<TSAPObject> Aggregate<TField>(Expression<Func<TSAPObject, TField>> selector, AggregateOperation operation, string? resultFieldName = null)
    {
        ArgumentNullException.ThrowIfNull(selector, nameof(selector));

        Aggregate(ExtractProperty(selector.Body), operation, resultFieldName);
        return this;
    }

    public SAPQuery<TSAPObject> GroupBy<TField>(Expression<Func<TSAPObject, TField>> selector)
    {
        if (selector.Body is MemberExpression memberExpression)
            _groupBy = memberExpression.Member.Name;

        if (selector.Body is NewExpression newExpression && newExpression.Members is not null)
            _groupBy = string.Join(',', newExpression.Members.Select(e => e.Name));

        return this;
    }

    public ICollection<TResult> ExecuteApply<TResult>()
        => ExecuteApplyAsync<TResult>().GetAwaiter().GetResult();

    public async Task<ICollection<TResult>> ExecuteApplyAsync<TResult>(CancellationToken cancellationToken = default)
    {
        string? applyStatement = GetApplyStatement();

        if (string.IsNullOrWhiteSpace(applyStatement))
            throw new InvalidOperationException("There is no aggregate or groupby operations");

        var request = BuildRequest();

        request.AddQueryParameter("$apply", applyStatement, false);

        var result = await Provider.ExecuteAsync<SapResponse<TResult>>(request, cancellationToken);

        if (result is null)
            return Array.Empty<TResult>();

        return result.Value;
    }

    public ICollection<TResult> ExecuteApply<TResult>(Expression<Func<TSAPObject, TResult>> typeBuilderStatement)
        => ExecuteApply<TResult>();

    public async Task<ICollection<TResult>> ExecuteApplyAsync<TResult>(Expression<Func<TSAPObject, TResult>> typeBuilderStatement, CancellationToken cancellationToken = default)
        => await ExecuteApplyAsync<TResult>(cancellationToken);

    private void Aggregate(string targetField, AggregateOperation operation, string? resultFieldName = null)
        => AggregateFields.Add($"{targetField} with {operation.GetValue()} as {resultFieldName ?? targetField}");

    private string? GetAggregateStatement() => _aggregateFields is null || _aggregateFields.Count == 0 ? null : $"aggregate({string.Join(',', _aggregateFields)})";

    private string? GetGroupByStatement() => string.IsNullOrWhiteSpace(_groupBy) ? null : $"groupby(({_groupBy}))";

    private string? GetApplyStatement()
    {
        string? aggregate = GetAggregateStatement();
        string? groupby = GetGroupByStatement();

        if (aggregate is null) return groupby;

        if (groupby is null) return aggregate;

        return $"{aggregate}/{groupby}";
    }

    private static string ExtractProperty(Expression expression)
    {
        if (expression is not MemberExpression memberExpression)
            throw new InvalidOperationException("Should select an entity property");

        return memberExpression.Member.Name;
    }

    internal RestRequest BuildRequest(bool includeInlineCount = false)
    {
        RestRequest request = new(ResourceName);

        if (!_isPaginated)
            request.AddHeader("Prefer", "odata.maxpagesize=0");

        if (_top is not null)
            request.AddQueryParameter("$top", _top.Value);

        if (_skip is not null)
            request.AddQueryParameter("$skip", _skip.Value);

        if (_select is not null)
            request.AddQueryParameter("$select", _select);

        if (_filter is not null)
            request.AddQueryParameter("$filter", _filter);

        if (_orderBy is not null)
            request.AddQueryParameter("$orderby", _orderBy);

        if (includeInlineCount)
            request.AddQueryParameter("$inlinecount", "allpages");

        return request;
    }
}
