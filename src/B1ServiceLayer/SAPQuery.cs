using RestSharp;
using System.Linq.Expressions;
using B1ServiceLayer.Models;

namespace B1ServiceLayer;

public class SAPQuery<T>
{
    public B1Service Provider { get; }
    public string ResourceName { get; }

    private bool _isPaginated = true;
    private int? _top;
    private int? _skip;
    private string? _select;
    private string? _filter;
    private string? _orderBy;

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

    public SAPQuery<T> Where(string predicate)
    {
        _filter = predicate;
        return this;
    }

    public SAPQuery<T> Where(Expression<Func<T, bool>> predicate)
    {
        _filter = SAPExpressionSerializer.Serialize(predicate.Body);
        return this;
    }

    public SAPQuery<TResult> Select<TResult>(Expression<Func<T, TResult>> predicate)
    {
        if (predicate.Body is MemberExpression memberExpression)
            _select = memberExpression.Member.Name;

        if (predicate.Body is NewExpression newExpression && newExpression.Members is not null)
            _select = string.Join(", ", newExpression.Members.Select(e => e.Name));

        return new SAPQuery<TResult>(Provider, ResourceName, _isPaginated, _top, _skip, _select, _filter, _orderBy);
    }

    public SAPQuery<T> Top(int qty)
    {
        _top = qty;
        return this;
    }

    public SAPQuery<T> Skip(int qty)
    {
        _skip = qty;
        return this;
    }

    public SAPQuery<T> NoPaginate()
    {
        _isPaginated = false;
        return this;
    }

    public SAPQuery<T> OrderBy(string orderBy)
    {
        _orderBy = orderBy;

        return this;
    }

    public SAPQuery<T> OrderBy(Expression<Func<T, object?>> predicate)
    {
        _orderBy = $"{ExtractProperty(predicate.Body)} asc";

        return this;
    }

    public SAPQuery<T> OrderByDesc(Expression<Func<T, object?>> predicate)
    {
        _orderBy = $"{ExtractProperty(predicate.Body)} desc";

        return this;
    }

    public SAPQuery<T> ThenOrderBy(Expression<Func<T, object?>> predicate)
    {
        if (string.IsNullOrEmpty(_orderBy))
            throw new InvalidOperationException($"Can't add secondary order by statement without the main statement, use {nameof(OrderBy)} instead");

        _orderBy += $", {ExtractProperty(predicate.Body)} asc";

        return this;
    }

    public SAPQuery<T> ThenOrderByDesc(Expression<Func<T, object?>> predicate)
    {
        if (string.IsNullOrEmpty(_orderBy))
            throw new InvalidOperationException($"Can't add secondary order by statement without the main statement, use {nameof(OrderByDesc)} instead");

        _orderBy += $", {ExtractProperty(predicate.Body)} desc";

        return this;
    }

    public ICollection<T> Get()
        => GetAsync().GetAwaiter().GetResult();

    public async Task<ICollection<T>> GetAsync()
    {
        var response = await Provider.ExecuteAsync<SapResponse<T>>(BuildRequest());

        if (response is null)
            return Array.Empty<T>();

        return response.Value;
    }

    public Paged<T>? GetWithCount()
        => GetWithCountAsync().GetAwaiter().GetResult();

    public async Task<Paged<T>?> GetWithCountAsync()
        => await Provider.ExecuteAsync<Paged<T>>(BuildRequest(includeInlineCount: true));

    public ICollection<TResult> Apply<TResult>(string applyStatement)
        => ApplyAsync<TResult>(applyStatement).GetAwaiter().GetResult();

    public async Task<ICollection<TResult>> ApplyAsync<TResult>(string applyStatement, CancellationToken cancellationToken = default)
    {
        var request = BuildRequest();

        request.AddQueryParameter("$apply", applyStatement, false);

        var response = await Provider.ExecuteAsync<SapResponse<TResult>>(request, cancellationToken);

        return response!.Value;
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
