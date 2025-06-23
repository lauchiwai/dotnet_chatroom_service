using Common.HttpClientResultDto;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Repositories.HttpClients;

public interface IApiClient
{
    /// <summary>
    /// 發送GET請求並回傳反序列化的響應內容
    /// </summary>
    Task<T> GetAsync<T>(string endpoint);

    /// <summary>
    /// 發送POST請求並回傳反序列化的響應內容
    /// </summary>
    Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest request);

    /// <summary>
    /// 發送DELETE請求並回傳反序列化的響應內容
    /// </summary>
    Task<TResponse> DeleteAsync<TResponse>(string endpoint);

    /// <summary>
    /// 發送POST請求並回傳HTTP狀態碼和反序列化的響應內容
    /// </summary>
    Task<(HttpStatusCode statusCode, TResponse response)> PostWithStatusAsync<TRequest, TResponse>(string endpoint, TRequest request);

    /// <summary>
    /// 發送GET請求並回傳HTTP狀態碼和反序列化的響應內容
    /// </summary>
    Task<(HttpStatusCode statusCode, T response)> GetWithStatusAsync<T>(string endpoint);

    /// <summary>
    /// 發送DELETE請求並回傳HTTP狀態碼和反序列化的響應內容
    /// </summary>
    Task<(HttpStatusCode statusCode, TResponse response)> DeleteWithStatusAsync<TResponse>(string endpoint);
}

public class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ITokenProvider _tokenProvider;

    public ApiClient(
        HttpClient httpClient,
        IConfiguration configuration,
        ITokenProvider tokenProvider)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _tokenProvider = tokenProvider;
        ConfigureHttpClient();
    }

    private void ConfigureHttpClient()
    {
        _httpClient.BaseAddress = new Uri(_configuration["ChatServiceApiSettings:BaseUrl"]);
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<T> GetAsync<T>(string endpoint)
    {
        var (_, response) = await GetWithStatusAsync<T>(endpoint);
        return response;
    }

    public async Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest request)
    {
        var (_, response) = await PostWithStatusAsync<TRequest, TResponse>(endpoint, request);
        return response;
    }

    public async Task<TResponse> DeleteAsync<TResponse>(string endpoint)
    {
        var (_, response) = await DeleteWithStatusAsync<TResponse>(endpoint);
        return response;
    }

    public async Task<(HttpStatusCode statusCode, TResponse response)> PostWithStatusAsync<TRequest, TResponse>(string endpoint, TRequest request)
    {
        var content = SerializeContent(request);
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = content
        };

        AddAuthorizationHeader(requestMessage);
        var response = await _httpClient.SendAsync(requestMessage);
        var responseObj = await DeserializeResponse<TResponse>(response);
        return (response.StatusCode, responseObj);
    }

    public async Task<(HttpStatusCode statusCode, T response)> GetWithStatusAsync<T>(string endpoint)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        AddAuthorizationHeader(request);
        var response = await _httpClient.SendAsync(request);
        var responseObj = await DeserializeResponse<T>(response);
        return (response.StatusCode, responseObj);
    }

    public async Task<(HttpStatusCode statusCode, TResponse response)> DeleteWithStatusAsync<TResponse>(string endpoint)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, endpoint);
        AddAuthorizationHeader(request);
        var response = await _httpClient.SendAsync(request);
        var responseObj = await DeserializeResponse<TResponse>(response);
        return (response.StatusCode, responseObj);
    }

    private void AddAuthorizationHeader(HttpRequestMessage request)
    {
        var token = _tokenProvider.GetToken();
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    private static StringContent SerializeContent<T>(T data)
    {
        var json = JsonSerializer.Serialize(data);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    private static async Task<T> DeserializeResponse<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStreamAsync();
        return await JsonSerializer.DeserializeAsync<T>(content) ?? throw new InvalidOperationException("Unable to deserialize response.");
    }
}
