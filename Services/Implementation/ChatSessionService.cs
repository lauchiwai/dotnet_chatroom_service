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

namespace Services.Implementation;
public class ChatSessionService : IChatSessionService
{
    private readonly MyDbContext _context;
    private readonly IUserHelper _jwtHelper;
    private readonly IChatServiceApiClient _chatHttpClient;

    public ChatSessionService(MyDbContext context, IUserHelper jwtHelper, IChatServiceApiClient chatHttpClient)
    {
        _context = context;
        _jwtHelper = jwtHelper;
        _chatHttpClient = chatHttpClient;
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
            _context.ChatSessions.Add(newChatSession);
            await _context.SaveChangesAsync();

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
            var chatSessionList = await _context.ChatSessions
                .Where(a => a.UserId == userInfo.UserId)
                .Select(a => new ChatSessionViewModel()
                {
                    SessionId = a.SessionId,
                    SessionName = a.SessionName ?? "",
                })
                .ToListAsync();

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

    public async Task<ResultDTO> GetChatSessionBySessionId(string sessionId)
    {
        var result = new ResultDTO() { IsSuccess = true };
        try
        {
            var userInfo = _jwtHelper.ParseToken<JwtUserInfo>();
            var chatSession = await _context.ChatSessions
                .Where(a => a.SessionId.ToString() == sessionId)
                .FirstAsync();

            if (chatSession == null)
            {
                result.IsSuccess = false;
                result.Code = 404;
                result.Message = "not found";
            }

            if (chatSession!.UserId != userInfo.UserId)
            {
                result.IsSuccess = false;
                result.Code = 403;
                result.Message = "You do not have permission to access this chat session";
            }

            result.Data = chatSession;
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
        var sessionResult = await GetChatSessionBySessionId(sessionId);
        if (!sessionResult.IsSuccess)
        {
            return sessionResult;
        }

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
        var getSessionResult = await GetChatSessionBySessionId(sessionId);
        if (!getSessionResult.IsSuccess)
        {
            return getSessionResult;
        }

        var deleteChatHistoryResult = await DeleteChatHistoryBySessionId(sessionId);
        if (!deleteChatHistoryResult.IsSuccess)
        {
            return deleteChatHistoryResult;
        }

        var sessionEntity = getSessionResult.Data as ChatSession;
        var deleteChatSessionResult = await DeleteChatSessionByEntity(sessionEntity);
        if (!deleteChatSessionResult.IsSuccess)
        {
            return deleteChatSessionResult;
        }

        return new ResultDTO() { 
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

    public async Task<ResultDTO> DeleteChatSessionByEntity(ChatSession chatSession)
    {
        var result = new ResultDTO() { IsSuccess = true };
        try
        {
            if (chatSession == null)
            {
                result.IsSuccess = false;
                result.Message = "chatSession is ArgumentNullException";
                return result;
            }

            _context.ChatSessions.Remove(chatSession);
            await _context.SaveChangesAsync();
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
}
