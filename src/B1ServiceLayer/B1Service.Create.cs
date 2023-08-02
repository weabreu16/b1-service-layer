using RestSharp;
using B1ServiceLayer.Interfaces;

namespace B1ServiceLayer;

public static partial class B1ServiceExtensions
{
    /// <summary>
    /// Create a new <typeparamref name="TSAPObject"/> instance.
    /// </summary>
    /// <remarks>
    /// This returns the created entity by default, if you do not want it, pass <paramref name="noContent"/> as <see langword="true"/>.
    /// </remarks>
    /// <typeparam name="TSAPObject"></typeparam>
    /// <param name="sap"></param>
    /// <param name="data"></param>
    /// <param name="noContent"></param>
    /// <returns>
    /// An instance of <typeparamref name="TSAPObject"/> or <see langword="null"/> if not created 
    /// or <paramref name="noContent"/> is passed as <see langword="true"/>.
    /// </returns>
    public static TSAPObject? Create<TSAPObject>(this B1Service sap, object data, bool noContent = false)
        where TSAPObject : class
            => sap.CreateAsync<TSAPObject>(data, noContent).GetAwaiter().GetResult();

    /// <summary>
    /// Create a new <typeparamref name="TSAPObject"/> instance.
    /// </summary>
    /// <remarks>
    /// This returns the created entity by default, if you do not want it, pass <paramref name="noContent"/> as <see langword="true"/>.
    /// </remarks>
    /// <typeparam name="TSAPObject"></typeparam>
    /// <param name="sap"></param>
    /// <param name="data"></param>
    /// <param name="noContent"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>
    /// An instance of <typeparamref name="TSAPObject"/> or <see langword="null"/> if not created 
    /// or <paramref name="noContent"/> is passed as <see langword="true"/>.
    /// </returns>
    public static async Task<TSAPObject?> CreateAsync<TSAPObject>(this B1Service sap, object data, bool noContent = false, CancellationToken cancellationToken = default) 
        where TSAPObject : class
            => await sap.ExecuteAsync<TSAPObject>(BuildRequest<TSAPObject>(data, noContent), cancellationToken);

    /// <summary>
    /// Add a new instance to target SAP resource collection.
    /// </summary>
    /// <remarks>
    /// This returns the created entity by default, if you do not want it, pass <paramref name="noContent"/> as <see langword="true"/>.
    /// </remarks>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="sap"></param>
    /// <param name="resourceName"></param>
    /// <param name="data"></param>
    /// <param name="noContent"></param>
    /// <returns>
    /// An instance of <typeparamref name="TResult"/> or <see langword="null"/> if not created 
    /// or <paramref name="noContent"/> is passed as <see langword="true"/>.
    /// </returns>
    public static TResult? Create<TResult>(this B1Service sap, string resourceName, object data, bool noContent = false)
        => sap.CreateAsync<TResult>(resourceName, data, noContent).GetAwaiter().GetResult();

    /// <summary>
    /// Add a new instance to target SAP resource collection.
    /// </summary>
    /// <remarks>
    /// This returns the created entity by default, if you do not want it, pass <paramref name="noContent"/> as <see langword="true"/>.
    /// </remarks>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="sap"></param>
    /// <param name="resourceName"></param>
    /// <param name="data"></param>
    /// <param name="noContent"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>
    /// An instance of <typeparamref name="TResult"/> or <see langword="null"/> if not created 
    /// or <paramref name="noContent"/> is passed as <see langword="true"/>.
    /// </returns>
    public static async Task<TResult?> CreateAsync<TResult>(this B1Service sap, string resourceName, object data, bool noContent = false, CancellationToken cancellationToken = default)
        => await sap.ExecuteAsync<TResult>(BuildRequest(resourceName, data, noContent), cancellationToken);

    /// <summary>
    /// Add a new instance to target SAP resource collection.
    /// </summary>
    /// <remarks>
    /// This returns the created entity by default, if you do not want it, pass <paramref name="noContent"/> as <see langword="true"/>.
    /// </remarks>
    /// <param name="sap"></param>
    /// <param name="resourceName"></param>
    /// <param name="data"></param>
    /// <param name="noContent"></param>
    /// <returns>A <see cref="RestResponse"/> instance.</returns>
    public static RestResponse ExecuteCreate(this B1Service sap, string resourceName, object data, bool noContent = false)
        => sap.ExecuteCreateAsync(resourceName, data, noContent).GetAwaiter().GetResult();

    /// <summary>
    /// Add a new instance to target SAP resource collection.
    /// </summary>
    /// <remarks>
    /// This returns the created entity by default, if you do not want it, pass <paramref name="noContent"/> as <see langword="true"/>.
    /// </remarks>
    /// <param name="sap"></param>
    /// <param name="resourceName"></param>
    /// <param name="data"></param>
    /// <param name="noContent"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>A <see cref="RestResponse"/> instance.</returns>
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
        where T: class
    {
        var request = B1Service.GetResourceRequest<T>(Method.Post);

        PrepareRequest(request, data, noContent);

        return request;
    }
}
