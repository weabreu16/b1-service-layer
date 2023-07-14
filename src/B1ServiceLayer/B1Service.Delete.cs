using RestSharp;
using B1ServiceLayer.Interfaces;

namespace B1ServiceLayer;

public static partial class B1ServiceExtensions
{
    public static RestResponse Delete<T>(this B1Service sap, object key)
        where T : ISAPObject, new()
            => sap.DeleteAsync<T>(key).GetAwaiter().GetResult();

    public static async Task<RestResponse> DeleteAsync<T>(this B1Service sap, object key, CancellationToken cancellationToken = default) 
        where T : ISAPObject, new()
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));

        RestRequest request = B1Service.GetResourceRequest<T>(key, Method.Delete);

        return await sap.RequestAsync(request, cancellationToken);
    }

    public static RestResponse Delete(this B1Service sap, string resourceName, object key)
        => sap.DeleteAsync(resourceName, key).GetAwaiter().GetResult();

    public static async Task<RestResponse> DeleteAsync(this B1Service sap, string resourceName, object key, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));
        ArgumentNullException.ThrowIfNullOrEmpty(resourceName, nameof(resourceName));

        RestRequest request = B1Service.GetResourceRequest(resourceName, key, Method.Delete);

        return await sap.RequestAsync(request, cancellationToken);
    }
}
