using RestSharp;
using B1ServiceLayer.Interfaces;

namespace B1ServiceLayer;

public static partial class B1ServiceExtensions
{
    /// <summary>
    /// Get an instance of <typeparamref name="TSAPObject"/> with the given <paramref name="key"/>.
    /// </summary>
    /// <remarks>
    /// If target instance has multiple primary keys, pass a class entity that represents the keys or use an anonymous type.
    /// The properties names should be equal to their respective SAP Entity Keys names.
    /// </remarks>
    /// <typeparam name="TSAPObject"></typeparam>
    /// <param name="sap"></param>
    /// <param name="key"></param>
    /// <returns>An instance of <typeparamref name="TSAPObject"/> or <see langword="null"/> if does not exist.</returns>
    public static TSAPObject? Find<TSAPObject>(this B1Service sap, object key)
        where TSAPObject : ISAPObject, new()
            => sap.FindAsync<TSAPObject>(key).GetAwaiter().GetResult();

    /// <summary>
    /// Get an instance of <typeparamref name="TSAPObject"/> with the given <paramref name="key"/>.
    /// </summary>
    /// <remarks>
    /// If target instance has multiple primary keys, pass a class entity that represents the keys or use an anonymous type.
    /// The properties names should be equal to their respective SAP Entity Keys names.
    /// </remarks>
    /// <typeparam name="TSAPObject"></typeparam>
    /// <param name="sap"></param>
    /// <param name="key"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>An instance of <typeparamref name="TSAPObject"/> or <see langword="null"/> if does not exist.</returns>
    public static async Task<TSAPObject?> FindAsync<TSAPObject>(this B1Service sap, object key, CancellationToken cancellationToken = default) 
        where TSAPObject : ISAPObject, new()
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));

        RestRequest request = B1Service.GetResourceRequest<TSAPObject>(key, Method.Get);

        return await sap.ExecuteAsync<TSAPObject>(request, cancellationToken);
    }

    /// <summary>
    /// Get an instance of target <paramref name="resourceName"/> with the given <paramref name="key"/>.
    /// </summary>
    /// <remarks>
    /// If target instance has multiple primary keys, pass a class entity that represents the keys or use an anonymous type.
    /// The properties names should be equal to their respective SAP Entity Keys names.
    /// </remarks>
    /// <typeparam name="TResult">The expected return type.</typeparam>
    /// <param name="sap"></param>
    /// <param name="resourceName"></param>
    /// <param name="key"></param>
    /// <returns>An instance of <typeparamref name="TResult"/> or <see langword="null"/> if does not exist.</returns>
    public static TResult? Find<TResult>(this B1Service sap, string resourceName, object key)
        => sap.FindAsync<TResult>(resourceName, key).GetAwaiter().GetResult();

    /// <summary>
    /// Get an instance of target <paramref name="resourceName"/> with the given <paramref name="key"/>.
    /// </summary>
    /// <remarks>
    /// If target instance has multiple primary keys, pass a class entity that represents the keys or use an anonymous type.
    /// The properties names should be equal to their respective SAP Entity Keys names.
    /// </remarks>
    /// <typeparam name="TResult">The expected return type.</typeparam>
    /// <param name="sap"></param>
    /// <param name="resourceName"></param>
    /// <param name="key"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>An instance of <typeparamref name="TResult"/> or <see langword="null"/> if does not exist.</returns>
    public static async Task<TResult?> FindAsync<TResult>(this B1Service sap, string resourceName, object key, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));
        ArgumentNullException.ThrowIfNull(resourceName, nameof(resourceName));

        RestRequest request = B1Service.GetResourceRequest(resourceName, key);

        return await sap.ExecuteAsync<TResult>(request, cancellationToken);
    }

    /// <summary>
    /// Get an instance of target <paramref name="resourceName"/> with the given <paramref name="key"/>.
    /// </summary>
    /// <remarks>
    /// If target instance has multiple primary keys, pass a class entity that represents the keys or use an anonymous type.
    /// The properties names should be equal to their respective SAP Entity Keys names.
    /// </remarks>
    /// <param name="sap"></param>
    /// <param name="resourceName"></param>
    /// <param name="key"></param>
    /// <returns>A <see cref="RestResponse"/> instance.</returns>
    public static RestResponse ExecuteFind(this B1Service sap, string resourceName, object key)
        => sap.ExecuteFindAsync(resourceName, key).GetAwaiter().GetResult();

    /// <summary>
    /// Get an instance of target <paramref name="resourceName"/> with the given <paramref name="key"/>.
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
    public static async Task<RestResponse> ExecuteFindAsync(this B1Service sap, string resourceName, object key, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));
        ArgumentNullException.ThrowIfNull(resourceName, nameof(resourceName));

        RestRequest request = B1Service.GetResourceRequest(resourceName, key);

        return await sap.RequestAsync(request, cancellationToken);
    }
}
