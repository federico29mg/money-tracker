using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

using Transactions.Api.Contracts;

namespace Transactions.IntegrationTests.Support;

public static class HttpClientExtensions
{
    // Same as the API: camelCase properties and enums as names.
    public static JsonSerializerOptions JsonOptions { get; } =
        new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    public static async Task<T> GetJsonAsync<T>(this HttpClient client, string url) =>
        await client.GetFromJsonAsync<T>(url, JsonOptions, TestContext.Current.CancellationToken)
        ?? throw new InvalidOperationException($"GET {url} returned no content.");

    public static Task<HttpResponseMessage> PostJsonAsync(this HttpClient client, string url, object body) =>
        client.PostAsJsonAsync(url, body, JsonOptions, TestContext.Current.CancellationToken);

    // Posts to an endpoint that creates something and returns the new id.
    public static async Task<Guid> CreateAsync(this HttpClient client, string url, object body)
    {
        using var response = await client.PostJsonAsync(url, body);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<CreatedResponse>(
            JsonOptions, TestContext.Current.CancellationToken);

        return created!.Id;
    }
}