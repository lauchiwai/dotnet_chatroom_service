using Common.Dto;
using Common.Helper.Interface;
using Common.Models;
using Repositories.HttpClients;
using Services.Interfaces;
using System.Text.Json;
using System.Text;
using Common.Helper.Implementation;
using Common.Params.Article;
using Common.ViewModels.Article;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace Services.Implementation;

public class ArticleService : IArticleService
{
    private readonly IMediator _mediator;
    private readonly IUserHelper _jwtHelper;
    private readonly IStreamClient _streamClient;
    private readonly IRepository<Article> _articleRepository;
    private readonly IRepository<OutboxMessage> _outboxMessageRepository;

    public ArticleService(
        IMediator mediator,
        IUserHelper jwtHelper,
        IStreamClient streamClient,
        IRepository<Article> articleRepository,
        IRepository<OutboxMessage> outboxMessageRepository)
    {
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
                UserId = userInfo.UserId,
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
            result.Code = 400;
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
            result.Code = 400;
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
