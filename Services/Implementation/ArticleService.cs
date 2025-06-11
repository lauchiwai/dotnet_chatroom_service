using Common.Dto;
using Common.Helper.Implementation;
using Common.Helper.Interface;
using Common.Models;
using Common.Params.Article;
using Common.ViewModels.Article;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Repositories.HttpClients;
using Repositories.MyDbContext;
using Services.Interfaces;
using System.Text;
using System.Text.Json;

namespace Services.Implementation;

public class ArticleService : IArticleService
{
    private readonly MyDbContext _context;
    private readonly IMediator _mediator;
    private readonly IUserHelper _jwtHelper;
    private readonly IStreamClient _streamClient;
    private readonly IRepository<Article> _articleRepository;
    private readonly IRepository<OutboxMessage> _outboxMessageRepository;

    public ArticleService(
        MyDbContext context,
        IMediator mediator,
        IUserHelper jwtHelper,
        IStreamClient streamClient,
        IRepository<Article> articleRepository,
        IRepository<OutboxMessage> outboxMessageRepository)
    {
        _context = context;
        _mediator = mediator;
        _jwtHelper = jwtHelper;
        _streamClient = streamClient;
        _articleRepository = articleRepository;
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
                OwnerId = userInfo.UserId,
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

    public async Task<ResultDTO> DeleteArticle(string articleId)
    {
        var result = new ResultDTO() { IsSuccess = true };
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var article = await _articleRepository.GetQueryable()
              .FirstOrDefaultAsync(a => a.ArticleID.ToString() == articleId);

            if (article == null)
            {
                result.IsSuccess = false;
                result.Code = 404;
                return result;
            }

            _articleRepository.Delete(article);

            var outboxMessage = new OutboxMessage
            {
                Id = Guid.NewGuid().ToString(),
                EventType = "ArticleDeleted",
                Payload = JsonSerializer.Serialize(new { ArticleId = articleId, CollectionName = "articles" }),
                CreatedTime = DateTime.UtcNow,
                IsPublished = false,
                RetryCount = 0
            };

            _outboxMessageRepository.Add(outboxMessage);

            await _articleRepository.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            result.IsSuccess = false;
            result.Code = 500;
            result.Message = ex.Message;
        }

        return result;
    }

    public async Task<ResultDTO> VectorizeArticle(VectorizeArticleParams vectorizeArticleParams)
    {
        return await _mediator.Send(new VectorizeArticleCommand
        {
            ArticleId = vectorizeArticleParams.ArticleId,
            CollectionName = vectorizeArticleParams.CollectionName
        });
    }

    public async Task<ResultDTO> GetArticle(int articleId)
    {
        var result = new ResultDTO() { IsSuccess = true };
        try
        {
            var userInfo = _jwtHelper.ParseToken<JwtUserInfo>();
            var article = await _articleRepository.GetQueryable()
              .Where(a => a.ArticleID == articleId && a.OwnerId == userInfo.UserId)
              .Select(a => new ArticleViewModel()
              {
                  ArticleId = a.ArticleID,
                  ArticleTitle = a.ArticleTitle,
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
            result.Code = 500;
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
              .Where(a => a.OwnerId == userInfo.UserId)
              .OrderByDescending(x => x.UpdateTime)
              .Select(a => new ArticleListViewModel()
              {
                  ArticleId = a.ArticleID,
                  ArticleTitle = a.ArticleTitle,
              }).ToListAsync();

            result.Data = articleList;
        }
        catch (Exception ex)
        {
            result.IsSuccess = false;
            result.Code = 500;
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
                fetchAiArticleParams,
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
            code = 400,
            message,
            eventType = "system_error"
        };

        var eventData = $"event: error\ndata: {JsonSerializer.Serialize(errorEvent)}\n\n";
        await stream.WriteAsync(Encoding.UTF8.GetBytes(eventData));
        await stream.FlushAsync();
    }
}
