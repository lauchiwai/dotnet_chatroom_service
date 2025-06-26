using Common.Commands;
using Common.Dto;
using Common.Helper.Implementation;
using Common.Helper.Interface;
using Common.Models;
using Common.Params.Article;
using Common.Params.Search;
using Common.ViewModels.Article;
using Common.ViewModels.Search;
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
    private readonly IRepository<Article_User> _articleUserRepository;
    private readonly IRepository<OutboxMessage> _outboxMessageRepository;


    public ArticleService(
        MyDbContext context,
        IMediator mediator,
        IUserHelper jwtHelper,
        IStreamClient streamClient,
        IRepository<Article> articleRepository,
        IRepository<Article_User> articleUserRepository,
        IRepository<OutboxMessage> outboxMessageRepository)
    {
        _context = context;
        _mediator = mediator;
        _jwtHelper = jwtHelper;
        _streamClient = streamClient;
        _articleRepository = articleRepository;
        _articleUserRepository = articleUserRepository;
        _outboxMessageRepository = outboxMessageRepository;
    }

    public async Task<ResultDTO> GenerateArticle(GenerateArticleParams generateArticleParams)
    {
        var result = new ResultDTO() { IsSuccess = true };

        using var transaction = await _context.Database.BeginTransactionAsync();

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

            var newArticleUser = new Article_User
            {
                UserId = userInfo.UserId,
                ArticleId = newArticle.ArticleID, 
                Progress = 0 
            };

            await _articleUserRepository.AddAsync(newArticleUser);
            await _articleUserRepository.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            result.Code = 500;
            result.IsSuccess = false;
            result.Message = ex.Message;
        }

        return result;
    }


    public async Task<ResultDTO> UpdateArticleReadingProgress(UpdateArticleReadingProgressParams progressParams)
    {
        var result = new ResultDTO() { IsSuccess = true };
        try
        {
            var userInfo = _jwtHelper.ParseToken<JwtUserInfo>();
            var association = await _articleUserRepository
                 .GetQueryable()
                 .FirstOrDefaultAsync(au =>
                     au.ArticleId == progressParams.ArticleId &&
                     au.UserId == userInfo.UserId
                 );

            if (association != null)
            {
                association.Progress = progressParams.Progress;
            }
            else
            {
                var newArticleUser = new Article_User
                {
                    UserId = userInfo.UserId,
                    ArticleId = progressParams.ArticleId,
                    Progress = progressParams.Progress
                };
                await _articleUserRepository.AddAsync(newArticleUser);
            }
            
            await _articleUserRepository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            result.Code = 500;
            result.IsSuccess = false;
            result.Message = ex.Message;
        }

        return result;
    }

    public async Task<ResultDTO> GetArticleReadingProgress(int articleId)
    {

        var result = new ResultDTO() { IsSuccess = true };
        try
        {
            var userInfo = _jwtHelper.ParseToken<JwtUserInfo>();
            var association = await _articleUserRepository
                 .GetQueryable()
                 .FirstOrDefaultAsync(au =>
                     au.ArticleId == articleId &&
                     au.UserId == userInfo.UserId
                 );

            if (association == null) {
                result.Data =  new ArticleReadingProgressViewmodel
                {
                    ArticleId = articleId,
                    Progress = 0
                };

            } else
            {
                result.Data = new ArticleReadingProgressViewmodel
                {
                    ArticleId = association.ArticleId,
                    Progress = association.Progress
                };
            }
        }
        catch (Exception ex)
        {
            result.Code = 500;
            result.IsSuccess = false;
            result.Message = ex.Message;
        }

        return result;
    }

    public async Task<ResultDTO> RequestArticleDeletion(int articleId)
    {
        var result = new ResultDTO() { IsSuccess = true };
        try
        {
            var sessionIds = await _context.Article_Chat_Session
                .Where(acs => acs.ArticleId == articleId)
                .Select(acs => acs.SessionID)
                .ToListAsync();

            var command = new DeleteArticleSession
            {
                ArticleId = articleId,
                SessionIds = sessionIds
            };

            return await _mediator.Send(command);
        }
        catch (Exception ex)
        {

            result.IsSuccess = false;
            result.Code = 500;
            result.Message = $"刪除請求失敗: {ex.Message}";
        }

        return result; 
    }

    public async Task<ResultDTO> DeleteArticle(int articleId)
    {
        var result = new ResultDTO() { IsSuccess = true };
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var article = await _articleRepository.GetQueryable()
                .Include(a => a.Article_Chat_Session)
                .Include(a => a.Article_User)
                .FirstOrDefaultAsync(a => a.ArticleID == articleId);

            if (article == null)
            {
                result.IsSuccess = false;
                result.Code = 404;
                result.Message = $"文章ID {articleId} 不存在";
                return result;
            }

            var sessionIds = article.Article_Chat_Session?
                .Select(acs => acs.SessionID)
                .ToList() ?? new List<int>();

            if (article.Article_User?.Any() == true)
            {
                _context.RemoveRange(article.Article_User);
            }

            _articleRepository.Delete(article);

            _outboxMessageRepository.Add(new OutboxMessage
            {
                Id = Guid.NewGuid().ToString(),
                EventType = "ArticleDeleted",
                Payload = JsonSerializer.Serialize(new
                {
                    ArticleId = articleId,
                    SessionIds = sessionIds,
                    CollectionName = "articles"
                }),
                CreatedTime = DateTime.UtcNow,
                IsPublished = false,
                RetryCount = 0
            });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            result.Data = sessionIds;
        }
        catch (Exception ex)
        {
            result.IsSuccess = false;
            result.Code = 500;
            result.Message = ex.Message;
            await transaction.RollbackAsync();
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
              .Where(a => a.ArticleID == articleId
                     && a.Article_User.Any(au => au.UserId == userInfo.UserId))
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

    public async Task<ResultDTO> GetArticleList(SearchParams? searchParams = null)
    {
        var result = new ResultDTO() { IsSuccess = true };
        try
        {
            var safeParams = searchParams ?? new SearchParams();

            safeParams.PageNumber = safeParams.PageNumber < 1 ? 1 : safeParams.PageNumber;
            safeParams.PageSize = safeParams.PageSize switch
            {
                < 1 => 20,
                > 100 => 100,
                _ => safeParams.PageSize
            };

            var userInfo = _jwtHelper.ParseToken<JwtUserInfo>();

            var baseQuery = _articleRepository.GetQueryable()
                .Where(a => a.Article_User.Any(au => au.UserId == userInfo.UserId));

            if (!string.IsNullOrWhiteSpace(safeParams.Keyword))
            {
                baseQuery = baseQuery.Where(a =>
                    a.ArticleTitle.Contains(safeParams.Keyword) ||
                    a.ArticleContent.Contains(safeParams.Keyword));
            }

            if (safeParams.StartDate.HasValue)
            {
                baseQuery = baseQuery.Where(a => a.UpdateTime >= safeParams.StartDate.Value);
            }
            if (safeParams.EndDate.HasValue)
            {
                baseQuery = baseQuery.Where(a => a.UpdateTime <= safeParams.EndDate.Value);
            }

            var orderedQuery = baseQuery.OrderByDescending(a => a.UpdateTime);

            var totalCount = await orderedQuery.CountAsync();

            var pagedResults = await orderedQuery
                .Select(a => new ArticleListViewModel()
                {
                    ArticleId = a.ArticleID,
                    ArticleTitle = a.ArticleTitle,
                })
                .Skip((safeParams.PageNumber - 1) * safeParams.PageSize)
                .Take(safeParams.PageSize)
                .ToListAsync();

            result.Data = new PagedViewModel<ArticleListViewModel>
            {
                Items = pagedResults,
                TotalCount = totalCount,
                PageNumber = safeParams.PageNumber,
                PageSize = safeParams.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)safeParams.PageSize)
            };
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
            code = 500,
            message,
            eventType = "system_error"
        };

        var eventData = $"event: error\ndata: {JsonSerializer.Serialize(errorEvent)}\n\n";
        await stream.WriteAsync(Encoding.UTF8.GetBytes(eventData));
        await stream.FlushAsync();
    }
}
