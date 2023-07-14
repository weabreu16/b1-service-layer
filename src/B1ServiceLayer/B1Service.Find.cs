using RestSharp;
using B1ServiceLayer.Interfaces;

namespace B1ServiceLayer;

public static partial class B1ServiceExtensions
{
    public static T? Find<T>(this B1Service sap, object key)
        where T : ISAPObject, new()
            => sap.FindAsync<T>(key).GetAwaiter().GetResult();

    public static async Task<T?> FindAsync<T>(this B1Service sap, object key, CancellationToken cancellationToken = default) 
        where T : ISAPObject, new()
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));

        RestRequest request = B1Service.GetResourceRequest<T>(key, Method.Get);

        return await sap.ExecuteAsync<T>(request, cancellationToken);
    }

    public static T? Find<T>(this B1Service sap, string resourceName, object key)
        => sap.FindAsync<T>(resourceName, key).GetAwaiter().GetResult();

    public static async Task<T?> FindAsync<T>(this B1Service sap, string resourceName, object key, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));
        ArgumentNullException.ThrowIfNull(resourceName, nameof(resourceName));

        RestRequest request = B1Service.GetResourceRequest(resourceName, key);

        return await sap.ExecuteAsync<T>(request, cancellationToken);
    }

    public static RestResponse ExecuteFind(this B1Service sap, string resourceName, object key)
        => sap.ExecuteFindAsync(resourceName, key).GetAwaiter().GetResult();

    public static async Task<RestResponse> ExecuteFindAsync(this B1Service sap, string resourceName, object key, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));
        ArgumentNullException.ThrowIfNull(resourceName, nameof(resourceName));

        RestRequest request = B1Service.GetResourceRequest(resourceName, key);

        return await sap.RequestAsync(request, cancellationToken);
    }
}
