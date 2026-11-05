using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using Users.Api.Contracts;
using Users.Application.Features.Login;

namespace Users.IntegrationTests.Support;

public static class HttpClientExtensions
{
    public const string Password = "correct horse battery";

    public static JsonSerializerOptions JsonOptions { get; } = new(JsonSerializerDefaults.Web);

    public static async Task<T> GetJsonAsync<T>(this HttpClient client, string url) =>
        await client.GetFromJsonAsync<T>(url, JsonOptions, TestContext.Current.CancellationToken)
        ?? throw new InvalidOperationException($"GET {url} returned no content.");

    public static Task<HttpResponseMessage> PostJsonAsync(this HttpClient client, string url, object body) =>
        client.PostAsJsonAsync(url, body, JsonOptions, TestContext.Current.CancellationToken);

    public static async Task<Guid> RegisterAsync(this HttpClient client, string email, string? currencyCode = null)
    {
        using var response = await client.PostJsonAsync(
            "/api/auth/register", new RegisterUserRequest(email, Password, "Ana", currencyCode));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var registered = await response.Content.ReadFromJsonAsync<RegisterUserResponse>(
            JsonOptions, TestContext.Current.CancellationToken);

        return registered!.Id;
    }

    // Logs in and sends the access token with every later request.
    public static async Task<LoginResponse> LogInAsync(this HttpClient client, string email)
    {
        using var response = await client.PostJsonAsync("/api/auth/login", new LoginRequest(email, Password));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var login = await response.Content.ReadFromJsonAsync<LoginResponse>(
            JsonOptions, TestContext.Current.CancellationToken);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.AccessToken);

        return login;
    }

    public static string NewEmail() => $"user-{Guid.NewGuid():N}@example.com";
}