using RestSharp;
using System.Linq.Expressions;
using B1ServiceLayer.Interfaces;

namespace B1ServiceLayer;

public static partial class B1ServiceExtensions
{
    public static int Count<T>(this B1Service sap, Expression<Func<T, bool>>? predicate = null) where T : ISAPObject, new()
        => sap.CountAsync(predicate).GetAwaiter().GetResult();

    public static async Task<int> CountAsync<T>(this B1Service sap, Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default) 
        where T : ISAPObject, new()
    {
        RestRequest request = new($"{B1Service.GetResourceName<T>()}/$count");

        if (predicate is not null)
            request.AddQueryParameter("$filter", SAPExpressionSerializer.Serialize(predicate.Body));

        return await sap.ExecuteAsync<int>(request, cancellationToken);
    }

    public static int Count(this B1Service sap, string resourceName, string? predicate = null)
        => sap.CountAsync(resourceName, predicate).GetAwaiter().GetResult();

    public static async Task<int> CountAsync(this B1Service sap, string resourceName, string? predicate = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(resourceName, nameof(resourceName));

        RestRequest request = B1Service.GetResourceRequest($"{resourceName}/$count");

        if (!string.IsNullOrWhiteSpace(predicate))
            request.AddQueryParameter("$filter", predicate);

        return await sap.ExecuteAsync<int>(request, cancellationToken);
    }
}
