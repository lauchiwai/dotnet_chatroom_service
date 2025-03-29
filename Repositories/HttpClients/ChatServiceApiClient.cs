using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Repositories.HttpClients;

public interface IChatServiceApiClient
{
    Task<T> GetAsync<T>(string endpoint);
    Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest request);
}

public class ChatServiceApiClient : IChatServiceApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ITokenProvider _tokenProvider;

    public ChatServiceApiClient(
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
        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        AddAuthorizationHeader(request);
        //Console.WriteLine("///////////////////////////");
        //var requestDetails = new StringBuilder();
        //requestDetails.AppendLine($"Method: {request.Method}");
        //requestDetails.AppendLine($"Request URI: {request.RequestUri}");

        //requestDetails.AppendLine("Headers:");
        //foreach (var header in request.Headers)
        //{
        //    requestDetails.AppendLine($"  {header.Key}: {string.Join(", ", header.Value)}");
        //}

        //Console.WriteLine(requestDetails.ToString());
        //Console.WriteLine("///////////////////////////");

        var response = await _httpClient.SendAsync(request);
        await EnsureSuccessStatusCode(response);
        return await DeserializeResponse<T>(response);
    }

    public async Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest request)
    {
        var content = SerializeContent(request);
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = content
        };
        AddAuthorizationHeader(requestMessage);
        var response = await _httpClient.SendAsync(requestMessage);
        await EnsureSuccessStatusCode(response);
        return await DeserializeResponse<TResponse>(response);
    }

    private void AddAuthorizationHeader(HttpRequestMessage request)
    {
        var token = _tokenProvider.GetToken();
        //Console.WriteLine("///////////////////////////");
        //Console.WriteLine("token : " + token);
        //Console.WriteLine("///////////////////////////");
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    private static async Task EnsureSuccessStatusCode(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                $"API request failed with status code {response.StatusCode}. Response: {errorContent}");
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
        return await JsonSerializer.DeserializeAsync<T>(content) ??
               throw new InvalidOperationException("Unable to deserialize response.");
    }
}