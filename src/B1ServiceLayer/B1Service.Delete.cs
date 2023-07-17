using RestSharp;
using B1ServiceLayer.Interfaces;

namespace B1ServiceLayer;

public static partial class B1ServiceExtensions
{
    /// <summary>
    /// Delete an instance of <typeparamref name="TSAPObject"/> with the given <paramref name="key"/>.
    /// </summary>
    /// <remarks>
    /// If target instance has multiple primary keys, pass a class entity that represents the keys or use an anonymous type.
    /// The properties names should be equal to their respective SAP Entity Keys names.
    /// </remarks>
    /// <typeparam name="TSAPObject"></typeparam>
    /// <param name="sap"></param>
    /// <param name="key"></param>
    /// <returns>A <see cref="RestResponse"/> instance.</returns>
    public static RestResponse Delete<TSAPObject>(this B1Service sap, object key)
        where TSAPObject : ISAPObject, new()
            => sap.DeleteAsync<TSAPObject>(key).GetAwaiter().GetResult();

    /// <summary>
    /// Delete an instance of <typeparamref name="TSAPObject"/> with the given <paramref name="key"/>.
    /// </summary>
    /// <remarks>
    /// If target instance has multiple primary keys, pass a class entity that represents the keys or use an anonymous type.
    /// The properties names should be equal to their respective SAP Entity Keys names.
    /// </remarks>
    /// <typeparam name="TSAPObject"></typeparam>
    /// <param name="sap"></param>
    /// <param name="key"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>A <see cref="RestResponse"/> instance.</returns>
    public static async Task<RestResponse> DeleteAsync<TSAPObject>(this B1Service sap, object key, CancellationToken cancellationToken = default) 
        where TSAPObject : ISAPObject, new()
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));

        RestRequest request = B1Service.GetResourceRequest<TSAPObject>(key, Method.Delete);

        return await sap.RequestAsync(request, cancellationToken);
    }

    /// <summary>
    /// Delete an instance of target <paramref name="resourceName"/> with the given <paramref name="key"/>.
    /// </summary>
    /// <remarks>
    /// If target instance has multiple primary keys, pass a class entity that represents the keys or use an anonymous type.
    /// The properties names should be equal to their respective SAP Entity Keys names.
    /// </remarks>
    /// <param name="sap"></param>
    /// <param name="resourceName"></param>
    /// <param name="key"></param>
    /// <returns>A <see cref="RestResponse"/> instance.</returns>
    public static RestResponse Delete(this B1Service sap, string resourceName, object key)
        => sap.DeleteAsync(resourceName, key).GetAwaiter().GetResult();

    /// <summary>
    /// Delete an instance of target <paramref name="resourceName"/> with the given <paramref name="key"/>.
    /// </summary>
    /// <remarks>
    /// If target instance has multiple primary keys, pass a class entity that represents the keys or use an anonymous type.
    /// The properties names should be equal to their respective SAP Entity Keys names.
    /// </remarks>
    /// <param name="sap"></param>
    /// <param name="resourceName"></param>
    /// <param name="key"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>A <see cref="RestResponse"/> instance.</returns>
    public static async Task<RestResponse> DeleteAsync(this B1Service sap, string resourceName, object key, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));
        ArgumentNullException.ThrowIfNullOrEmpty(resourceName, nameof(resourceName));

        RestRequest request = B1Service.GetResourceRequest(resourceName, key, Method.Delete);

        return await sap.RequestAsync(request, cancellationToken);
    }
}
