using RestSharp;
using B1ServiceLayer.Models;

namespace B1ServiceLayer.Queries;

public class SAPQuery
{
    public B1Service Provider { get; }

    public string ResourceName { get; }

    private bool _isPaginated = true;
    private bool _withInlineCount = false;
    private int? _top;
    private int? _skip;
    private string? _select;
    private string? _filter;
    private string? _orderBy;
    private string[]? _expands;
    private string[]? _groupBy;
    private string[]? _aggregate;

    internal SAPQuery(B1Service b1Service, string resourceName)
    {
        Provider = b1Service;
        ResourceName = resourceName;
    }

    public SAPQuery NoPaginate()
    {
        _isPaginated = false;
        return this;
    }

    public SAPQuery Where(string predicate)
    {
        _filter = predicate;
        return this;
    }

    public SAPQuery OrderBy(string statement)
    {
        _orderBy = statement;
        return this;
    }

    public SAPQuery Expand(params string[] expands)
    {
        _expands = expands;
        return this;
    }

    public SAPQuery Select(string selector)
    {
        _select = selector;
        return this;
    }

    public SAPQuery Top(int top)
    {
        _top = top;
        return this;
    }

    public SAPQuery Skip(int skip)
    {
        _skip = skip;
        return this;
    }

    public SAPQuery GroupBy(params string[] fields)
    {
        _groupBy = fields;
        return this;
    }

    public SAPQuery Aggregate(string[] aggregates)
    {
        _aggregate = aggregates;
        return this;
    }

    public SAPQuery WithInlineCount()
    {
        _withInlineCount = true;
        return this;
    }

    public ICollection<TResult> Execute<TResult>()
        => ExecuteAsync<TResult>().GetAwaiter().GetResult();

    public async Task<ICollection<TResult>> ExecuteAsync<TResult>()
    {
        var result = await Provider.ExecuteAsync<SAPResponse<List<TResult>>>(BuildRequest());

        return result?.Value is not null ? result.Value : Array.Empty<TResult>();
    }

    internal RestRequest BuildRequest()
    {
        RestRequest request = new(ResourceName);

        if (_expands is not null)
            request.AddQueryParameter("$expand", string.Join(',', _expands));

        if (!_isPaginated)
            request.AddHeader("Prefer", "odata.maxpagesize=0");

        if (_top.HasValue)
            request.AddQueryParameter("$top", _top.Value);

        if (_skip.HasValue)
            request.AddQueryParameter("$skip", _skip.Value);

        if (!string.IsNullOrWhiteSpace(_select))
            request.AddQueryParameter("$select", _select);

        if (!string.IsNullOrWhiteSpace(_filter))
            request.AddQueryParameter("$filter", _filter);

        if (!string.IsNullOrWhiteSpace(_orderBy))
            request.AddQueryParameter("$orderby", _orderBy);

        if (_withInlineCount)
            request.AddQueryParameter("$inlinecount", "allpages");

        string? apply = GetApply();

        if (!string.IsNullOrWhiteSpace(apply))
            request.AddQueryParameter("$apply", apply);

        return request;
    }

    private string? GetGroupBy() => _groupBy is null ? null : $"groupby(({_groupBy}))";

    private string? GetAggregate() => _aggregate is null ? null : $"aggregate({_aggregate})";

    private string? GetApply()
    {
        string? aggregate = GetAggregate();
        string? groupBy = GetGroupBy();

        if (aggregate is null)
            return groupBy;

        if (groupBy is null)
            return aggregate;

        return $"{aggregate}/{groupBy}";
    }
}
