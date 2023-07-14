using RestSharp;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
using B1ServiceLayer.Exceptions;
using B1ServiceLayer.Models;

namespace B1ServiceLayer.Extensions;

public static class RestResponseExtensions
{
    public static void ThrowIfFailed<T>(this RestResponse<T> response)
    {
        if (!response.IsSuccessful || (response.Data is null && response.StatusCode != HttpStatusCode.NoContent))
        {
            if (response.Content is not null)
                Throw(response.Content);

            throw new SAPException(response.ErrorMessage, response.ErrorException);
        }
    }

    public static void ThrowIfFailed(this RestResponse response)
    {
        if (response.IsSuccessful) return;

        if (!string.IsNullOrWhiteSpace(response.Content))
            Throw(response.Content);

        throw new SAPException(response.ErrorMessage, response.ErrorException);
    }

    [DoesNotReturn]
    private static void Throw(string content)
    {
        try
        {
            var errorResponse = JsonSerializer.Deserialize<SAPErrorResponse>(content, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (errorResponse is null || errorResponse.Error is null)
                throw new SAPException(content);

            throw new SAPException($"({errorResponse.Error.Code}) {errorResponse.Error.Message}");
        }
        catch (NotSupportedException)
        {
            throw new SAPException(content);
        }
        catch (JsonException)
        {
            throw new SAPException(content);
        }
    }
}
