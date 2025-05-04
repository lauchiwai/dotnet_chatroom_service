using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

namespace Repositories.HttpClients;

public interface IStreamClient
{
    public Task PostStreamAsync<TRequest>(string endpoint, TRequest request, Stream responseStream, CancellationToken cancellationToken);
}

public class StreamClient : IStreamClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ITokenProvider _tokenProvider;

    public StreamClient(
        HttpClient httpClient,
        IConfiguration configuration,
        ITokenProvider tokenProvider)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _tokenProvider = tokenProvider;
        ConfigureStreamingClient();
    }

    private void ConfigureStreamingClient()
    {
        _httpClient.BaseAddress = new Uri(_configuration["ChatServiceApiSettings:BaseUrl"]);
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
    }

    public async Task PostStreamAsync<TRequest>(string endpoint, TRequest request, Stream responseStream, CancellationToken cancellationToken)
    {
        using var requestMessage = CreateStreamRequest(endpoint, request);
        AddAuthorizationHeader(requestMessage);

        using var response = await SendStreamRequest(requestMessage);

        response.EnsureSuccessStatusCode();
        await response.Content.CopyToAsync(responseStream, cancellationToken);
    }

    private HttpRequestMessage CreateStreamRequest<TRequest>(string endpoint, TRequest request)
    {
        return new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = SerializeStreamContent(request),
            Headers =
                {
                    {"X-Request-ID", Guid.NewGuid().ToString()},
                    {"X-Client-Version", "1.0.0"}
                }
        };
    }

    private void AddAuthorizationHeader(HttpRequestMessage request)
    {
        var token = _tokenProvider.GetToken();
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    private async Task<HttpResponseMessage> SendStreamRequest(HttpRequestMessage request)
    {
        var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                $"Stream request failed: {response.StatusCode} - {errorContent}");
        }

        return response;
    }

    private static StringContent SerializeStreamContent<T>(T data)
    {
        var json = JsonSerializer.Serialize(data);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }
}
