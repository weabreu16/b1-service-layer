using RestSharp;
using RestSharp.Serializers.Json;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using B1ServiceLayer.Exceptions;
using B1ServiceLayer.Extensions;
using B1ServiceLayer.Interfaces;

namespace B1ServiceLayer;

/// <summary>
/// <para>Service that establish connection with SAP B1 Service Layer.</para>
/// <para>Each method create a new session with the Service Layer.</para>
/// </summary>
public class B1Service: IDisposable
{
    private readonly RestClient _restClient;
    private readonly ISAPCredentials _credentials;
    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNamingPolicy = null,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// </summary>
    /// <param name="baseUrl">SAP B1 Service Layer URL, for example: <code>http://localhost:50000/b1s/v2</code></param>
    /// <param name="credentials"></param>
    public B1Service(string baseUrl, ISAPCredentials credentials)
    {
        _restClient = BuildRestClient(baseUrl);
        _credentials = credentials;
    }

    public B1Service(ISAPConfig config)
    {
        _restClient = BuildRestClient(config.BaseUrl);
        _credentials = config;
    }

    /// <summary>
    /// Create a new SAP Query for target <typeparamref name="TSAPObject"/>.
    /// </summary>
    /// <typeparam name="TSAPObject"></typeparam>
    /// <returns></returns>
    public SAPQuery<TSAPObject> Set<TSAPObject>() where TSAPObject : ISAPObject, new()
        => new(this, GetResourceName<TSAPObject>());

    /// <summary>
    /// Execute an http request against target resource name.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="resourceName">SAP Resource Name, like BusinessPartners.</param>
    /// <param name="method">Http Method.</param>
    /// <returns></returns>
    public async Task<TResult?> ExecuteMethod<TResult>(string resourceName, Method method = Method.Get)
    {
        RestRequest request = new(resourceName, method);

        return await ExecuteAsync<TResult>(request);
    }

    internal static RestRequest GetResourceRequest<T>(Method method = Method.Get) where T : ISAPObject, new()
        => new($"{GetResourceName<T>()}", method);

    internal static RestRequest GetResourceRequest<T>(object key, Method method = Method.Get) where T : ISAPObject, new()
        => new($"{GetResourceName<T>()}({SAPExpressionSerializer.GetValueAsQueryFormatted(key)})", method);

    internal static RestRequest GetResourceRequest(string resourceName, Method method = Method.Get)
        => new(resourceName, method);

    internal static RestRequest GetResourceRequest(string resourceName, object key, Method method = Method.Get)
        => new($"{resourceName}({SAPExpressionSerializer.GetValueAsQueryFormatted(key)})", method);

    internal static string GetResourceName<T>() where T : ISAPObject, new()
        => new T().GetResourceName();

    /// <summary>
    /// LogIn against SAP B1 Service Layer and execute a request.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>The expected data.</returns>
    /// <exception cref="SAPException"></exception>
    public async Task<T?> ExecuteAsync<T>(RestRequest request, CancellationToken cancellationToken = default)
        => (await RequestAsync<T>(request, cancellationToken)).Data;

    /// <summary>
    /// LogIn against SAP B1 Service Layer and execute a request.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>A <see cref="RestResponse{T}"/> instance with the expected data.</returns>
    /// <exception cref="SAPException"></exception>
    public async Task<RestResponse<T>> RequestAsync<T>(RestRequest request, CancellationToken cancellationToken = default)
    {
        await LogInAsync();

        var res = await _restClient.ExecuteAsync<T>(request, cancellationToken);

        res.ThrowIfFailed();

        return res;
    }

    /// <summary>
    /// LogIn against SAP B1 Service Layer and execute a request.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>A <see cref="RestResponse"/> instance.</returns>
    /// <exception cref="SAPException"></exception>
    public async Task<RestResponse> RequestAsync(RestRequest request, CancellationToken cancellationToken = default)
    {
        await LogInAsync();

        var res = await _restClient.ExecuteAsync(request, cancellationToken);

        res.ThrowIfFailed();

        return res;
    }

    public void Dispose()
    {
        LogOut();
        _restClient.Dispose();
        GC.SuppressFinalize(this);
    }

    private static RestClient BuildRestClient(string baseUrl)
    {
        var options = new RestClientOptions()
        {
            BaseUrl = new Uri(baseUrl),
            RemoteCertificateValidationCallback = (a, b, c, d) => true,
            CookieContainer = new CookieContainer()
        };

        return new RestClient(options, configureSerialization: s => s.UseSystemTextJson(_serializerOptions));
    }

    /// <summary>
    /// Authenticate against the SAP B1 Service Layer to create a new session for the following requests.
    /// </summary>
    /// <exception cref="SAPUnauthorizedException"></exception>
    public void LogIn()
        => LogInAsync().GetAwaiter().GetResult();

    /// <summary>
    /// Authenticate against the SAP B1 Service Layer to create a new session for the following requests.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="SAPUnauthorizedException"></exception>
    public async Task LogInAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            RestRequest request = new("Login");
            request.AddBody(_credentials);
            var response = await _restClient.ExecutePostAsync(request, cancellationToken);

            response.ThrowIfFailed();
        }
        catch (Exception ex)
        {
            throw new SAPUnauthorizedException(ex.Message, ex);
        }
    }

    /// <summary>
    /// Log Out from SAP B1 Service Layer.
    /// </summary>
    /// <exception cref="SAPException"></exception>
    public void LogOut()
        => LogOutAsync().GetAwaiter().GetResult();

    /// <summary>
    /// Log Out from SAP B1 Service Layer.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SAPException"></exception>
    public async Task LogOutAsync()
    {
        try
        {
            RestRequest request = new("Logout");
            var response = await _restClient.ExecutePostAsync(request);

            response.ThrowIfFailed();
        }
        catch (Exception ex)
        {
            throw new SAPException(ex.Message, ex);
        }
    }
}
