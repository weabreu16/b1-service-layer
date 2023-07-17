using RestSharp;
using B1ServiceLayer.Interfaces;

namespace B1ServiceLayer;

public static partial class B1ServiceExtensions
{
    /// <summary>
    /// Update an instance of <typeparamref name="TSAPObject"/> with the given <paramref name="key"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If target instance has multiple primary keys, pass a class entity that represents the keys or use an anonymous type.
    /// The properties names should be equal to their respective SAP Entity Keys names.
    /// </para>
    /// <para>
    /// This execute a Patch http method by default. If Put is needed, pass the param <paramref name="usePut"/> as <see langword="true"/>.
    /// </para>
    /// </remarks>
    /// <typeparam name="TSAPObject"></typeparam>
    /// <param name="sap"></param>
    /// <param name="key"></param>
    /// <param name="data"></param>
    /// <param name="usePut"></param>
    /// <returns>A <see cref="RestResponse"/> instance.</returns>
    public static RestResponse Update<TSAPObject>(this B1Service sap, object key, object data, bool usePut = false)
        where TSAPObject : ISAPObject, new()
            => sap.UpdateAsync<TSAPObject>(key, data, usePut).GetAwaiter().GetResult();

    /// <summary>
    /// Update an instance of <typeparamref name="TSAPObject"/> with the given <paramref name="key"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If target instance has multiple primary keys, pass a class entity that represents the keys or use an anonymous type.
    /// The properties names should be equal to their respective SAP Entity Keys names.
    /// </para>
    /// <para>
    /// This execute a Patch http method by default. If Put is needed, pass the param <paramref name="usePut"/> as <see langword="true"/>.
    /// </para>
    /// </remarks>
    /// <typeparam name="TSAPObject"></typeparam>
    /// <param name="sap"></param>
    /// <param name="key"></param>
    /// <param name="data"></param>
    /// <param name="usePut"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>A <see cref="RestResponse"/> instance.</returns>
    public static async Task<RestResponse> UpdateAsync<TSAPObject>(this B1Service sap, object key, object data, bool usePut = false, CancellationToken cancellationToken = default)
        where TSAPObject : ISAPObject, new()
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));
        ArgumentNullException.ThrowIfNull(data, nameof(data));

        var method = usePut ? Method.Put : Method.Patch;
        RestRequest request = B1Service.GetResourceRequest<TSAPObject>(key, method);

        request.AddBody(data);

        return await sap.RequestAsync(request, cancellationToken);
    }

    /// <summary>
    /// Update an instance of target SAP resource with the given <paramref name="key"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If target instance has multiple primary keys, pass a class entity that represents the keys or use an anonymous type.
    /// The properties names should be equal to their respective SAP Entity Keys names.
    /// </para>
    /// <para>
    /// This execute a Patch http method by default. If Put is needed, pass the param <paramref name="usePut"/> as <see langword="true"/>.
    /// </para>
    /// </remarks>
    /// <param name="sap"></param>
    /// <param name="resourceName"></param>
    /// <param name="key"></param>
    /// <param name="data"></param>
    /// <param name="usePut"></param>
    /// <returns>A <see cref="RestResponse"/> instance.</returns>
    public static RestResponse Update(this B1Service sap, string resourceName, object key, object data, bool usePut = false)
        => sap.UpdateAsync(resourceName, key, data, usePut).GetAwaiter().GetResult();

    /// <summary>
    /// Update an instance of target SAP resource with the given <paramref name="key"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If target instance has multiple primary keys, pass a class entity that represents the keys or use an anonymous type.
    /// The properties names should be equal to their respective SAP Entity Keys names.
    /// </para>
    /// <para>
    /// This execute a Patch http method by default. If Put is needed, pass the param <paramref name="usePut"/> as <see langword="true"/>.
    /// </para>
    /// </remarks>
    /// <param name="sap"></param>
    /// <param name="resourceName"></param>
    /// <param name="key"></param>
    /// <param name="data"></param>
    /// <param name="usePut"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>A <see cref="RestResponse"/> instance.</returns>
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
