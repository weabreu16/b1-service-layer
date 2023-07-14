using RestSharp;
using B1ServiceLayer.Interfaces;

namespace B1ServiceLayer;

public static partial class B1ServiceExtensions
{
    public static T? Create<T>(this B1Service sap, object data, bool noContent = false)
        where T : ISAPObject, new()
            => sap.CreateAsync<T>(data, noContent).GetAwaiter().GetResult();

    public static async Task<T?> CreateAsync<T>(this B1Service sap, object data, bool noContent = false, CancellationToken cancellationToken = default) 
        where T : ISAPObject, new()
            => await sap.ExecuteAsync<T>(BuildRequest<T>(data, noContent), cancellationToken);

    public static T? Create<T>(this B1Service sap, string resourceName, object data, bool noContent = false)
        => sap.CreateAsync<T>(resourceName, data, noContent).GetAwaiter().GetResult();

    public static async Task<T?> CreateAsync<T>(this B1Service sap, string resourceName, object data, bool noContent = false, CancellationToken cancellationToken = default)
        => await sap.ExecuteAsync<T>(BuildRequest(resourceName, data, noContent), cancellationToken);

    public static RestResponse ExecuteCreate(this B1Service sap, string resourceName, object data, bool noContent = false)
        => sap.ExecuteCreateAsync(resourceName, data, noContent).GetAwaiter().GetResult();

    public static async Task<RestResponse> ExecuteCreateAsync(this B1Service sap, string resourceName, object data, bool noContent = false, CancellationToken cancellationToken = default)
        => await sap.RequestAsync(BuildRequest(resourceName, data, noContent), cancellationToken);

    private static void PrepareRequest(RestRequest request, object data, bool noContent = false)
    {
        request.AddBody(data);

        if (noContent)
            request.AddHeader("Prefer", "return-no-content");
    }

    private static RestRequest BuildRequest(string resourceName, object data, bool noContent = false)
    {
        var request = B1Service.GetResourceRequest(resourceName, Method.Post);

        PrepareRequest(request, data, noContent);

        return request;
    }

    private static RestRequest BuildRequest<T>(object data, bool noContent = false)
        where T: ISAPObject, new()
    {
        var request = B1Service.GetResourceRequest<T>(Method.Post);

        PrepareRequest(request, data, noContent);

        return request;
    }
}
