using RestSharp;
using B1ServiceLayer.Interfaces;

namespace B1ServiceLayer;

public static partial class B1ServiceExtensions
{
    public static RestResponse Update<T>(this B1Service sap, object key, object data, bool usePut = false)
        where T : ISAPObject, new()
            => sap.UpdateAsync<T>(key, data, usePut).GetAwaiter().GetResult();

    public static async Task<RestResponse> UpdateAsync<T>(this B1Service sap, object key, object data, bool usePut = false, CancellationToken cancellationToken = default)
        where T : ISAPObject, new()
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));
        ArgumentNullException.ThrowIfNull(data, nameof(data));

        var method = usePut ? Method.Put : Method.Patch;
        RestRequest request = B1Service.GetResourceRequest<T>(key, method);

        request.AddBody(data);

        return await sap.RequestAsync(request, cancellationToken);
    }

    public static RestResponse Update(this B1Service sap, string resourceName, object key, object data, bool usePut = false)
        => sap.UpdateAsync(resourceName, key, data, usePut).GetAwaiter().GetResult();

    public static async Task<RestResponse> UpdateAsync(this B1Service sap, string resourceName, object key, object data, bool usePut = false, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));
        ArgumentNullException.ThrowIfNull(data, nameof(data));

        var method = usePut ? Method.Put : Method.Patch;
        RestRequest request = B1Service.GetResourceRequest(resourceName, key, method);

        request.AddBody(data);

        return await sap.RequestAsync(request, cancellationToken);
    }
}
