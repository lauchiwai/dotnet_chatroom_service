using Common.Dto;
using Common.Helper.Implementation;
using Common.Helper.Interface;
using Common.HttpClientDto;
using Common.HttpClientResultDto;
using Common.Models;
using Common.Params.Chat;
using Common.ViewModels.Chat;
using Microsoft.EntityFrameworkCore;
using Repositories.HttpClients;
using Repositories.MyDbContext;
using Services.Interfaces;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Services.Implementation;
public class ChatService : IChatService
{
    private readonly MyDbContext _context;
    private readonly IUserHelper _jwtHelper;
    private readonly IApiClient _httpClient;
    private readonly IStreamClient _streamClient;
    private readonly IRepository<ChatSession> _chatSessionRepository;
    private readonly IRepository<OutboxMessage> _outboxMessageRepository;
    private readonly string _userTimeZoneId;

    public ChatService(
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
        _userTimeZoneId = "Asia/Hong_Kong";
    }

    public async Task<ResultDTO> GenerateChatSession(ChatSessionParams param)
    {
        var result = new ResultDTO() { IsSuccess = true };
        try
        {
            var userInfo = _jwtHelper.ParseToken<JwtUserInfo>();
            var newChatSession = new ChatSession()
            {
                UserId = userInfo.UserId,
                SessionName = param.ChatSessionName ?? GenerateChatSessionName(),
                UpdateTime = DateTime.UtcNow
            };

            await _chatSessionRepository.AddAsync(newChatSession);
            await _chatSessionRepository.SaveChangesAsync();

            var chatSessionViewModel = new ChatSessionViewModel()
            {
                SessionId = newChatSession.SessionId,
                SessionName = newChatSession.SessionName,
            };

            result.Data = chatSessionViewModel;
        }
        catch (Exception ex)
        {
            result.IsSuccess = false;
            result.Code = 500;
            result.Message = ex.Message;
        }

        return result;
    }

    public async Task<ResultDTO> GenerateRagChatSession(int articleId)
    {
        var result = new ResultDTO() { IsSuccess = true };
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var userInfo = _jwtHelper.ParseToken<JwtUserInfo>();

            var newChatSession = new ChatSession
            {
                UserId = userInfo.UserId,
                SessionName = GenerateChatSessionName(),
                UpdateTime = DateTime.UtcNow
            };

            var articleChatSession = new Article_Chat_Session
            {
                ArticleId = articleId,
                Session = newChatSession
            };

            _context.Article_Chat_Session.Add(articleChatSession);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            result.Data = new ChatSessionViewModel()
            {
                SessionId = articleChatSession.Session.SessionId,
                SessionName = articleChatSession.Session.SessionName
            };
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

    public string GenerateChatSessionName()
    {
        var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(_userTimeZoneId);
        var userLocalTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, userTimeZone);
        return userLocalTime.ToString("yyyy年MM月dd日HH時mm分", CultureInfo.InvariantCulture);
    }

    public async Task<ResultDTO> GetChatSessionList()
    {
        var result = new ResultDTO() { IsSuccess = true };
        try
        {
            var userInfo = _jwtHelper.ParseToken<JwtUserInfo>();
            var chatSessionList = await _chatSessionRepository.GetQueryable()
                .Where(a => a.UserId == userInfo.UserId)
                .OrderByDescending(x => x.UpdateTime)
                .Select(a => new ChatSessionViewModel()
                {
                    SessionId = a.SessionId,
                    SessionName = a.SessionName ?? "",
                }).ToListAsync();

            result.Data = chatSessionList;
        }
        catch (Exception ex)
        {
            result.IsSuccess = false;
            result.Code = 500;
            result.Message = ex.Message;
        }

        return result;
    }

    public async Task<ResultDTO> GetSceneChatSessionList()
    {
        var result = new ResultDTO { IsSuccess = true };
        try
        {
            var userInfo = _jwtHelper.ParseToken<JwtUserInfo>();

            var associatedSessionIds = _context.Article_Chat_Session
                .Select(acs => acs.SessionID)
                .Distinct();

            var orphanedSessions = await _chatSessionRepository.GetQueryable()
                .Where(cs =>
                    cs.UserId == userInfo.UserId &&
                    !associatedSessionIds.Contains(cs.SessionId))
                .OrderByDescending(cs => cs.UpdateTime)
                .Select(cs => new ChatSessionViewModel
                {
                    SessionId = cs.SessionId,
                    SessionName = cs.SessionName ?? "Unnamed Session"
                })
                .ToListAsync();

            result.Data = orphanedSessions;
        }
        catch (Exception ex)
        {
            result.IsSuccess = false;
            result.Code = 500;
            result.Message = $"Internal error: {ex.Message}";
        }
        return result;
    }


    public async Task<ResultDTO> GetRagChatSessionListByArticleId(int articleId)
    {
        var result = new ResultDTO() { IsSuccess = true };
        try
        {
            var userInfo = _jwtHelper.ParseToken<JwtUserInfo>();

            var chatSessionList = await _chatSessionRepository.GetQueryable()
                .Where(cs => cs.UserId == userInfo.UserId)
                .Join(
                    _context.Article_Chat_Session,
                    cs => cs.SessionId,
                    acs => acs.SessionID,
                    (cs, acs) => new { cs, acs }
                )
                .Where(x => x.acs.ArticleId == articleId)
                .OrderByDescending(x => x.cs.UpdateTime)
                .Select(x => new ChatSessionViewModel()
                {
                    SessionId = x.cs.SessionId,
                    SessionName = x.cs.SessionName ?? ""
                })
                .ToListAsync();

            result.Data = chatSessionList;
        }
        catch (Exception ex)
        {
            result.IsSuccess = false;
            result.Code = 500;
            result.Message = ex.Message;
        }
        return result;
    }

    public async Task<ResultDTO> ValidateChatPermission(int sessionId)
    {
        var result = new ResultDTO() { IsSuccess = true };
        try
        {
            var userInfo = _jwtHelper.ParseToken<JwtUserInfo>();

            var chatSession = await _chatSessionRepository.GetQueryable()
               .FirstOrDefaultAsync(a => a.SessionId == sessionId);

            if (chatSession == null)
            {
                result.IsSuccess = false;
                result.Code = 404;
                result.Message = "not found";
                return result;
            }

            if (chatSession!.UserId != userInfo.UserId)
            {
                result.IsSuccess = false;
                result.Code = 403;
                result.Message = "You do not have permission to access this chat session";
                return result;
            }
        }
        catch (Exception ex)
        {
            result.IsSuccess = false;
            result.Code = 500;
            result.Message = ex.Message;
        }

        return result;
    }

    public async Task<ResultDTO> GetChatHistory(int sessionId)
    {
        var (statusCode, response) = await _httpClient.GetWithStatusAsync<ChatServiceHttpClientResultDto>(
            $"Chat/getChatHistoryBySessionId/{sessionId}"
        );

        return new ResultDTO()
        {
            IsSuccess = response.success,
            Code = (int)statusCode,
            Data = response.data,
            Message = response.message ?? string.Empty
        };
    }

    public async Task<ResultDTO> DeleteChatData(int sessionId)
    {
        var result = new ResultDTO() { IsSuccess = true };

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var articleSessions = await _context.Article_Chat_Session
               .Where(acs => acs.SessionID == sessionId)
               .ToListAsync();


            if (articleSessions.Any())
            {
                _context.Article_Chat_Session.RemoveRange(articleSessions);
            }

            var chatSession = await _context.ChatSession.FindAsync(sessionId);
            if (chatSession == null)
            {
                result.IsSuccess = false;
                result.Code = 404;
                result.Message = "chat session does not exist";
                return result;
            }

            _context.ChatSession.Remove(chatSession);

            var outboxMessage = new OutboxMessage
            {
                Id = Guid.NewGuid().ToString(),
                EventType = "ChatSessionDeleted",
                Payload = JsonSerializer.Serialize(new
                {
                    SessionId = sessionId,
                    DeletedAt = DateTime.UtcNow
                }),
                CreatedTime = DateTime.UtcNow,
                IsPublished = false,
                RetryCount = 0
            };

            _outboxMessageRepository.Add(outboxMessage);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            result.IsSuccess = false;
            result.Code = 500;
            result.Message = $"delete fail: {ex.Message}";
        }

        return result;
    }

    public async Task<ResultDTO> RefreshChatSessionTime(int sessionId)
    {
        var result = new ResultDTO() { IsSuccess = true };
        try
        {
            var chatSession = await _chatSessionRepository.GetQueryable()
               .Where(a => a.SessionId == sessionId)
               .FirstOrDefaultAsync();

            if (chatSession == null)
            {
                result.IsSuccess = false;
                result.Code = 404;
                return result;
            }

            chatSession.UpdateTime = DateTime.UtcNow;

            _chatSessionRepository.Update(chatSession);
            await _chatSessionRepository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            result.IsSuccess = false;
            result.Message = ex.Message;
            result.Code = 500;
        }

        return result;
    }

    public async Task ChatStream(Stream outputStream, ChatParams chatParams, CancellationToken cancellationToken)
    {
        try
        {
            var validationResult = await ValidateChatPermission(chatParams.ChatSessionId);
            if (!validationResult.IsSuccess)
            {
                await SendValidationError(outputStream, validationResult);
                return;
            }

            var userInfo = _jwtHelper.ParseToken<JwtUserInfo>();
            var chatHttpRequest = new ChatHttpRequest()
            {
                UserId = userInfo.UserId,
                ArticleId = chatParams.ArticleId,
                ChatSessionId = chatParams.ChatSessionId,
                CollectionName = chatParams.CollectionName,
                Message = chatParams.Message,
            };

            await _streamClient.PostStreamAsync(
                "/Chat/chat_stream",
                chatHttpRequest,
                outputStream,
                cancellationToken
            );
        }
        catch (Exception ex)
        {
            await SendErrorEvent(outputStream, ex.Message);
        }
    }

    public async Task SummaryStream(Stream outputStream, SummaryParams summaryParams, CancellationToken cancellationToken)
    {
        try
        {
            var validationResult = await ValidateChatPermission(summaryParams.ChatSessionId);
            if (!validationResult.IsSuccess)
            {
                await SendValidationError(outputStream, validationResult);
                return;
            }

            var userInfo = _jwtHelper.ParseToken<JwtUserInfo>();
            var summaryHttpRequest = new SummaryHttpRequest()
            {
                UserId = userInfo.UserId,
                ArticleId = summaryParams.ArticleId,
                ChatSessionId = summaryParams.ChatSessionId,
                CollectionName = summaryParams.CollectionName,
            };


            await _streamClient.PostStreamAsync(
                "/Chat/summary_stream",
                summaryHttpRequest,
                outputStream,
                cancellationToken
            );
        }
        catch (Exception ex)
        {
            await SendErrorEvent(outputStream, ex.Message);
        }
    }

    public async Task SceneChatStream(Stream outputStream, SceneChatParams sceneChatParams, CancellationToken cancellationToken)
    {
        try
        {
            var validationResult = await ValidateChatPermission(sceneChatParams.ChatSessionId);
            if (!validationResult.IsSuccess)
            {
                await SendValidationError(outputStream, validationResult);
                return;
            }

            var userInfo = _jwtHelper.ParseToken<JwtUserInfo>();
            var summaryHttpRequest = new SceneChatHttpRequest()
            {
                UserId = userInfo.UserId,
                ChatSessionId = sceneChatParams.ChatSessionId,
                Message = sceneChatParams.Message,
            };

            await _streamClient.PostStreamAsync(
                "/Chat/scene_chat_stream",
                summaryHttpRequest,
                outputStream,
                cancellationToken
            );
        }
        catch (Exception ex)
        {
            await SendErrorEvent(outputStream, ex.Message);
        }
    }

    private async Task SendValidationError(Stream stream, ResultDTO result)
    {
        var errorEvent = new
        {
            code = 500,
            message = result.Message,
            eventType = "permission_denied"
        };

        var eventData = $"event: error\ndata: {JsonSerializer.Serialize(errorEvent)}\n\n";
        await stream.WriteAsync(Encoding.UTF8.GetBytes(eventData));
        await stream.FlushAsync();
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
