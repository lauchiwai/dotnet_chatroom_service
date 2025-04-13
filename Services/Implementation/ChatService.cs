using Common.Dto;
using Common.Helper.Implementation;
using Common.Helper.Interface;
using Common.Models;
using Common.Params;
using Common.ViewModels;
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
    private readonly IChatServiceApiClient _chatHttpClient;
    private readonly IChatServiceStreamClient _chatServiceStreamClient;
    private readonly IRepository<ChatSession> _chatSessionRepository;

    public ChatService(
        MyDbContext context,
        IUserHelper jwtHelper,
        IChatServiceApiClient chatHttpClient,
        IChatServiceStreamClient chatServiceStreamClient,
         IRepository<ChatSession> chatSessionRepository)
    {
        _context = context;
        _jwtHelper = jwtHelper;
        _chatHttpClient = chatHttpClient;
        _chatServiceStreamClient = chatServiceStreamClient;
        _chatSessionRepository = chatSessionRepository;
    }

    public async Task<ResultDTO> GenerateChatSession(string userTimeZoneId = "Asia/Hong_Kong")
    {
        var result = new ResultDTO() { IsSuccess = true };
        try
        {
            var userInfo = _jwtHelper.ParseToken<JwtUserInfo>();
            var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(userTimeZoneId);
            var userLocalTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, userTimeZone);
            var newChatSession = new ChatSession()
            {
                UserId = userInfo.UserId,
                SessionName = userLocalTime.ToString("yyyy年MM月dd日HH時mm分", CultureInfo.InvariantCulture),
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
            result.Message = ex.Message;
        }

        return result;
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
            result.Message = ex.Message;
        }

        return result;
    }

    public async Task<ResultDTO> CheackChatHttpClientHealth()
    {
        var response = await _chatHttpClient.GetAsync<ChatServiceHttpClientResultDto>("health");
        return new ResultDTO()
        {
            IsSuccess = response.success,
            Data = response.data,
            Message = response.message
        };
    }

    public async Task<ResultDTO> ValidateChatPermission(string sessionId)
    {
        var result = new ResultDTO() { IsSuccess = true };
        try
        {
            var userInfo = _jwtHelper.ParseToken<JwtUserInfo>();

            var chatSession = await _chatSessionRepository.GetQueryable()
               .FirstOrDefaultAsync(a => a.SessionId.ToString() == sessionId);

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
            result.Message = ex.Message;
        }

        return result;
    }

    public async Task<ResultDTO> GetChatHistoryBySessionId(string sessionId)
    {
        var response = await _chatHttpClient.GetAsync<ChatServiceHttpClientResultDto>($"Chat/getChatHistoryBySessionId/{sessionId}");
        return new ResultDTO()
        {
            IsSuccess = response.success,
            Data = response.data,
            Message = response.message
        };
    }

    public async Task<ResultDTO> DeleteChatData(string sessionId)
    {
        var deleteChatHistoryResult = await DeleteChatHistoryBySessionId(sessionId);
        if (!deleteChatHistoryResult.IsSuccess)
        {
            return deleteChatHistoryResult;
        }

        var deleteChatSessionResult = await DeleteChatSessionBySessionId(sessionId);
        if (!deleteChatSessionResult.IsSuccess)
        {
            return deleteChatSessionResult;
        }

        return new ResultDTO()
        {
            IsSuccess = true,
            Message = "delete successs"
        };
    }

    public async Task<ResultDTO> DeleteChatHistoryBySessionId(string sessionId)
    {
        var response = await _chatHttpClient.DeleteAsync<ChatServiceHttpClientResultDto>($"Chat/deleteChatHistoryBySessionId/{sessionId}");
        return new ResultDTO()
        {
            IsSuccess = response.success,
            Data = response.data,
            Message = response.message,
        };
    }

    public async Task<ResultDTO> DeleteChatSessionBySessionId(string sessionId)
    {
        var result = new ResultDTO() { IsSuccess = true };
        try
        {
            var chatSession = await _chatSessionRepository.GetQueryable()
               .FirstAsync(a => a.SessionId.ToString() == sessionId);

            _chatSessionRepository.Delete(chatSession);
            await _chatSessionRepository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            result.IsSuccess = false;
            result.Message = ex.Message;
        }

        return result;
    }

    public async Task<ResultDTO> Chat(ChatParams chatParams)
    {
        var response = await _chatHttpClient.PostAsync<ChatParams, ChatServiceHttpClientResultDto>("Chat/chat", chatParams);
        return new ResultDTO()
        {
            IsSuccess = response.success,
            Data = response.data,
            Message = response.message
        };
    }

    public async Task<ResultDTO> RefreshChatSessionTime(string sessionId)
    {
        var result = new ResultDTO() { IsSuccess = true };
        try
        {
            var chatSession = await _chatSessionRepository.GetQueryable()
               .Where(a => a.SessionId.ToString() == sessionId)
               .FirstOrDefaultAsync();

            if(chatSession == null)
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

            await _chatServiceStreamClient.PostStreamAsync(
                "/Chat/chat_stream",
                chatParams,
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
            code = result.Code,
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
