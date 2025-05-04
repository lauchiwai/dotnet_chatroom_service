using Common.Dto;
using Common.Helper.Interface;
using Common.Models;
using Common.Params;
using Repositories.HttpClients;
using Repositories.MyDbContext;
using Services.Interfaces;
using System.Text.Json;
using System.Text;

namespace Services.Implementation;

public class ArticleService : IArticleService
{
    private readonly MyDbContext _context;
    private readonly IUserHelper _jwtHelper;
    private readonly IApiClient _httpClient;
    private readonly IStreamClient _streamClient;
    private readonly IRepository<ChatSession> _chatSessionRepository;
    private readonly IRepository<OutboxMessage> _outboxMessageRepository;
    public ArticleService(
        MyDbContext context,
        IUserHelper jwtHelper,
        IApiClient httpClient,
        IStreamClient streamClient,
        IRepository<ChatSession> chatSessionRepository,
        IRepository<OutboxMessage> outboxMessageRepository)
    {
        _context = context;
        _jwtHelper = jwtHelper;
        _httpClient = httpClient;
        _streamClient = streamClient;
        _chatSessionRepository = chatSessionRepository;
        _outboxMessageRepository = outboxMessageRepository;
    }

    public async Task SteamGenerateArticle(Stream outputStream, ArticleGenerationParams articleGenerationParams, CancellationToken cancellationToken)
    {
        try
        {
            await _streamClient.PostStreamAsync(
                "/Article/stream_generate_article",
                articleGenerationParams,
                outputStream,
                cancellationToken
            );
        }
        catch (Exception ex)
        {
            await SendErrorEvent(outputStream, ex.Message);
        }
    }


    private async Task SendErrorEvent(Stream stream, string message)
    {
        var errorEvent = new
        {
            code = 500,
            message,
            eventType = "system_error"
        };

        var eventData = $"event: error\ndata: {JsonSerializer.Serialize(errorEvent)}\n\n";
        await stream.WriteAsync(Encoding.UTF8.GetBytes(eventData));
        await stream.FlushAsync();
    }
}
