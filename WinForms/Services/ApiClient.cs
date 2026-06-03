using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using WinForms.Models;

namespace WinForms.Services;

// Thrown when the BackEnd returns a non-success status code.
public class ApiException : Exception
{
    public int StatusCode { get; }
    public ApiException(int statusCode, string message) : base(message) => StatusCode = statusCode;
}

// Thin wrapper around HttpClient that talks to the BackEnd REST API and
// keeps the JWT bearer token for authenticated requests.
public class ApiClient
{
    private HttpClient _http;
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public string? Token { get; private set; }
    public bool IsAuthenticated => !string.IsNullOrEmpty(Token);

    public ApiClient(string baseUrl) => _http = CreateClient(baseUrl);

    private static HttpClient CreateClient(string baseUrl)
    {
        // The BackEnd uses the ASP.NET dev HTTPS certificate. For a local
        // developer tool we accept it without strict validation.
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };
        if (!baseUrl.EndsWith('/')) baseUrl += "/";
        return new HttpClient(handler) { BaseAddress = new Uri(baseUrl) };
    }

    // Point the client at a different BackEnd URL (re-applies the token).
    public void SetBaseUrl(string baseUrl)
    {
        _http.Dispose();
        _http = CreateClient(baseUrl);
        if (!string.IsNullOrEmpty(Token))
            _http.DefaultRequestHeaders.Authorization = new("Bearer", Token);
    }

    public void SetToken(string? token)
    {
        Token = token;
        _http.DefaultRequestHeaders.Authorization =
            string.IsNullOrEmpty(token) ? null : new("Bearer", token);
    }

    // --- Auth -------------------------------------------------------------
    public Task<AuthResponse> LoginAsync(string email, string password) =>
        PostForResultAsync<AuthResponse>("api/Auth/login", new { email, password });

    public Task<AuthResponse> RegisterAsync(string firstName, string lastName, string email, string password, string role) =>
        PostForResultAsync<AuthResponse>("api/Auth/register",
            new { firstName, lastName, email, password, role });

    // --- Generic verbs ----------------------------------------------------
    public async Task<List<T>> GetListAsync<T>(string url)
        => await DeserializeAsync<List<T>>(await SendAsync(HttpMethod.Get, url)) ?? new List<T>();

    public async Task<T?> GetAsync<T>(string url)
        => await DeserializeAsync<T>(await SendAsync(HttpMethod.Get, url));

    public async Task PostAsync(string url, object body)
        => await SendAsync(HttpMethod.Post, url, body);

    public async Task PutAsync(string url, object body)
        => await SendAsync(HttpMethod.Put, url, body);

    public async Task DeleteAsync(string url)
        => await SendAsync(HttpMethod.Delete, url);

    // --- Internals --------------------------------------------------------
    private async Task<T> PostForResultAsync<T>(string url, object body)
    {
        var json = await SendAsync(HttpMethod.Post, url, body, throwOnError: false);
        var result = await DeserializeAsync<T>(json);
        return result ?? throw new ApiException(500, "Empty response from server.");
    }

    private async Task<string> SendAsync(HttpMethod method, string url, object? body = null, bool throwOnError = true)
    {
        using var request = new HttpRequestMessage(method, url);
        if (body is not null)
            request.Content = new StringContent(JsonSerializer.Serialize(body, JsonOpts), Encoding.UTF8, "application/json");

        HttpResponseMessage response;
        try
        {
            response = await _http.SendAsync(request);
        }
        catch (Exception ex)
        {
            throw new ApiException(0, $"Could not reach the server.\n{ex.Message}");
        }

        var text = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode && throwOnError)
            throw new ApiException((int)response.StatusCode, BuildError(response.StatusCode, text));

        return text;
    }

    private static string BuildError(System.Net.HttpStatusCode code, string body)
    {
        var message = string.IsNullOrWhiteSpace(body) ? code.ToString() : body;
        // The BackEnd's exception middleware returns { "statusCode": .., "message": .. }.
        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("message", out var m))
                message = m.GetString() ?? message;
        }
        catch { /* not JSON, use raw body */ }
        return $"({(int)code}) {message}";
    }

    private static Task<T?> DeserializeAsync<T>(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return Task.FromResult<T?>(default);
        return Task.FromResult(JsonSerializer.Deserialize<T>(json, JsonOpts));
    }
}
