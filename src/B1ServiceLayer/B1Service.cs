using RestSharp;
using RestSharp.Serializers.Json;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using B1ServiceLayer.Exceptions;
using B1ServiceLayer.Extensions;
using B1ServiceLayer.Interfaces;
using B1ServiceLayer.Queries;
using B1ServiceLayer.Attributes;
using System.Linq.Expressions;
using System.Reflection;
using B1ServiceLayer.Expressions;
using B1ServiceLayer.Helpers;
using B1ServiceLayer.Visitors;
using B1ServiceLayer.Models;

namespace B1ServiceLayer;

/// <summary>
/// <para>Service that establish connection with SAP B1 Service Layer.</para>
/// <para>Each method create a new session with the Service Layer.</para>
/// </summary>
public class B1Service: IAsyncQueryProvider, IDisposable
{
    private readonly RestClient _restClient;
    private readonly ISAPCredentials _credentials;
    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNamingPolicy = null,
        Converters = { new JsonStringEnumConverter() }
    };

    private static readonly Type _sapEntityType = typeof(SAPEntityAttribute);
    private static readonly MethodInfo _selectMethod = typeof(Enumerable).GetMethods(BindingFlags.Public | BindingFlags.Static)
        .First(e => e.Name == "Select");
    private static readonly MethodInfo _executeMethod
        = typeof(B1Service).GetMethods().First(e => e.Name == nameof(Execute));
    private static readonly MethodInfo _asyncExecuteMethod
        = typeof(B1Service).GetMethod(nameof(ExecuteAsync))!;

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
    /// Create a new typeless SAP query for target <paramref name="entity"/> resource name.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public SAPQuery Query(string entity)
        => new(this, entity);

    /// <summary>
    /// Create a new typeless SAP query with cross join applied.
    /// </summary>
    /// <remarks>
    /// Entities names should be separated by (,).
    /// Example:
    ///     <example>Orders, BusinessPartners</example>
    /// </remarks>
    /// <param name="entities"></param>
    /// <param name="expands"></param>
    /// <returns></returns>
    public SAPQuery CrossJoin(string entities, params string[] expands)
        => new SAPQuery(this, $"$crossjoin({entities})").Expand(expands);

    /// <summary>
    /// Create a new SAP queryable for target <typeparamref name="TSAPEntity"/>.
    /// </summary>
    /// <typeparam name="TSAPEntity"></typeparam>
    /// <returns></returns>
    public IQueryable<TSAPEntity> Query<TSAPEntity>() where TSAPEntity : class
        => new SAPQueryable<TSAPEntity>(this, GetResourceName<TSAPEntity>());

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

    internal static RestRequest GetResourceRequest<TSAPEntity>(Method method = Method.Get) where TSAPEntity : class
        => new($"{GetResourceName<TSAPEntity>()}", method);

    internal static RestRequest GetResourceRequest<TSAPEntity>(object key, Method method = Method.Get) where TSAPEntity : class
        => new($"{GetResourceName<TSAPEntity>()}({SAPExpressionSerializer.GetValueAsQueryFormatted(key)})", method);

    internal static RestRequest GetResourceRequest(string resourceName, Method method = Method.Get)
        => new(resourceName, method);

    internal static RestRequest GetResourceRequest(string resourceName, object key, Method method = Method.Get)
        => new($"{resourceName}({SAPExpressionSerializer.GetValueAsQueryFormatted(key)})", method);

    internal static string GetResourceName<TSAPEntity>() where TSAPEntity : class
    {
        var sapObject = (SAPEntityAttribute?)Attribute.GetCustomAttribute(typeof(TSAPEntity), _sapEntityType)
            ?? throw new InvalidOperationException($"{typeof(TSAPEntity)} does not implement {_sapEntityType.Name}");

        return sapObject.ResourceName;
    }

    /// <summary>
    /// LogIn against SAP B1 Service Layer and execute a request.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="request"></param>
    /// <returns>The expected data.</returns>
    /// <exception cref="SAPException"></exception>
    public TResult? Execute<TResult>(RestRequest request)
        => ExecuteAsync<TResult>(request).GetAwaiter().GetResult();

    /// <summary>
    /// LogIn against SAP B1 Service Layer and execute a request.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>The expected data.</returns>
    /// <exception cref="SAPException"></exception>
    public async Task<TResult?> ExecuteAsync<TResult>(RestRequest request, CancellationToken cancellationToken = default)
        => (await RequestAsync<TResult>(request, cancellationToken)).Data;

    /// <summary>
    /// LogIn against SAP B1 Service Layer and execute a request.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>A <see cref="RestResponse{T}"/> instance with the expected data.</returns>
    /// <exception cref="SAPException"></exception>
    public async Task<RestResponse<TResult>> RequestAsync<TResult>(RestRequest request, CancellationToken cancellationToken = default)
    {
        await LogInAsync();

        var res = await _restClient.ExecuteAsync<TResult>(request, cancellationToken);

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

    async Task<TResult> IAsyncQueryProvider.ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
    {
        var query = (SAPQueryExpression)new SAPQueryExpressionVisitor().Visit(expression);

        var request = RequestFactory.Create(query);

        if (query.Select is not null && query.Select.Body is MemberExpression)
        {
            var listType = typeof(List<>).MakeGenericType(query.Resources.First().Key);
            var genericResponseType = typeof(SAPResponse<>).MakeGenericType(listType);
            dynamic selectResponse = _asyncExecuteMethod.MakeGenericMethod(genericResponseType).Invoke(this, new object[] { request, cancellationToken })!;
            await selectResponse;
            selectResponse = selectResponse.GetAwaiter().GetResult();

            return ExecuteSelect<TResult>(query.ElementType, selectResponse.Value, query.Select);
        }

        if (query.IsCounting || query.InlineCount)
            return (await ExecuteAsync<TResult>(request, cancellationToken))!;

        var response = await ExecuteAsync<SAPResponse<TResult>>(request, cancellationToken);

        return response!.Value!;
    }

    IQueryable IQueryProvider.CreateQuery(Expression expression)
    {
        throw new NotImplementedException();
    }

    IQueryable<TElement> IQueryProvider.CreateQuery<TElement>(Expression expression)
        => new SAPQueryable<TElement>(this, expression);

    object? IQueryProvider.Execute(Expression expression)
        => throw new NotImplementedException();

    TResult IQueryProvider.Execute<TResult>(Expression expression)
    {
        var query = (SAPQueryExpression)new SAPQueryExpressionVisitor().Visit(expression);

        if (expression.NodeType == ExpressionType.Call && query.QueryableExecutor is not null)
        {
            var queryableMethodCall = Expression.Call(
                query.QueryableExecutor.MakeGenericMethod(typeof(TResult)),
                Expression.Constant(
                    ((IQueryProvider)this)
                        .Execute<IEnumerable<TResult>>(query)));

            return Expression.Lambda<Func<TResult>>(queryableMethodCall).Compile()();
        }

        var request = RequestFactory.Create(query);

        if (query.Select is not null && query.Select.Body is MemberExpression)
        {
            var listType = typeof(List<>).MakeGenericType(query.Resources.First().Key);
            var genericResponseType = typeof(SAPResponse<>).MakeGenericType(listType);
            dynamic response = _executeMethod.MakeGenericMethod(genericResponseType).Invoke(this, new object[] { request })!;

            return ExecuteSelect<TResult>(query.ElementType, response.Value, query.Select);
        }

        if (query.IsCounting || query.InlineCount)
            return Execute<TResult>(request)!;

        return Execute<SAPResponse<TResult>>(request)!.Value!;
    }

    private static TResult ExecuteSelect<TResult>(Type parameterType, dynamic source, LambdaExpression selector)
    {
        var resultTypeGenericArguments = typeof(TResult).GetGenericArguments();

        if (!resultTypeGenericArguments.Any())
            throw new InvalidOperationException($"{typeof(TResult).Name} has no generic arguments");

        var call = Expression.Call(_selectMethod.MakeGenericMethod(parameterType, resultTypeGenericArguments[0]), Expression.Constant(source), selector);
        LambdaExpression lambda = Expression.Lambda<Func<IEnumerable<dynamic>>>(call);

        var func = lambda.Compile();

        return (TResult)func.DynamicInvoke()!;
    }
}
