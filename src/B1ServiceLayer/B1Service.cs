using RestSharp;
using RestSharp.Serializers.Json;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using B1ServiceLayer.Exceptions;
using B1ServiceLayer.Extensions;
using B1ServiceLayer.Interfaces;

namespace B1ServiceLayer;

public class B1Service: IDisposable
{
    private readonly RestClient _restClient;
    private readonly ISAPCredentials _credentials;
    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNamingPolicy = null,
        Converters = { new JsonStringEnumConverter() }
    };

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

    public SAPQuery<T> Set<T>() where T : ISAPObject, new()
        => new(this, GetResourceName<T>());

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

    public async Task<T?> ExecuteAsync<T>(RestRequest request, CancellationToken cancellationToken = default)
        => (await RequestAsync<T>(request, cancellationToken)).Data;

    public async Task<RestResponse<T>> RequestAsync<T>(RestRequest request, CancellationToken cancellationToken = default)
    {
        await LogInAsync();

        var res = await _restClient.ExecuteAsync<T>(request, cancellationToken);

        res.ThrowIfFailed();

        return res;
    }

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

    public void LogIn()
        => LogInAsync().GetAwaiter().GetResult();

    public async Task LogInAsync()
    {
        try
        {
            RestRequest request = new("Login");
            request.AddBody(_credentials);
            var response = await _restClient.ExecutePostAsync(request);

            response.ThrowIfFailed();
        }
        catch (Exception ex)
        {
            throw new SAPUnauthorizedException(ex.Message, ex);
        }
    }

    public void LogOut()
        => LogOutAsync().GetAwaiter().GetResult();

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
