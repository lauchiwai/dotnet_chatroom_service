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

    public async Task<ResultDTO> GenerateArticle(GenerateArticleParams generateArticleParams)
    {
        var result = new ResultDTO() { IsSuccess = true };
        try
        {
            var userInfo = _jwtHelper.ParseToken<JwtUserInfo>();
            var newArticle = new Article()
            {
                UserId = userInfo.UserId,
                ArticleTitle = generateArticleParams.ArticleTitle,
                ArticleContent = generateArticleParams.ArticleContent,
                UpdateTime = DateTime.UtcNow
            };

            await _articleRepository.AddAsync(newArticle);
            await _articleRepository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            result.IsSuccess = false;
            result.Message = ex.Message;
        }

        return result;
    }

    public async Task<ResultDTO> GetArticle(int articleId)
    {
        var result = new ResultDTO() { IsSuccess = true };
        try
        {
            var userInfo = _jwtHelper.ParseToken<JwtUserInfo>();
            var article = await _articleRepository.GetQueryable()
              .Where(a => a.ArticleID == articleId && a.UserId == userInfo.UserId)
              .Select(a => new ArticleViewModel()
              {
                  ArticleId = a.ArticleID,
                  ArticleContent = a.ArticleContent,
              }).FirstOrDefaultAsync();

            if (article == null)
            {
                result.IsSuccess = false;
                result.Message = "Article not found.";
                return result;
            }

            result.Data = article;
        }
        catch (Exception ex)
        {
            result.IsSuccess = false;
            result.Message = ex.Message;
        }

        return result;
    }

    public async Task<ResultDTO> GetArticleList()
    {
        var result = new ResultDTO() { IsSuccess = true };
        try
        {
            var userInfo = _jwtHelper.ParseToken<JwtUserInfo>();
            var articleList = await _articleRepository.GetQueryable()
              .Where(a => a.UserId == userInfo.UserId)
              .OrderByDescending(x => x.UpdateTime)
              .Select(a => new ArticleViewModel()
              {
                  ArticleId = a.ArticleID,
                  ArticleContent = a.ArticleContent,
              }).ToListAsync();

            result.Data = articleList;
        }
        catch (Exception ex)
        {
            result.IsSuccess = false;
            result.Message = ex.Message;
        }

        return result;
    }

    public async Task SteamFeatchAiArticle(Stream outputStream, FetchAiArticleParams fetchAiArticleParams, CancellationToken cancellationToken)
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
