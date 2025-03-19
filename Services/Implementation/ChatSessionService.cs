using Common.Dto;
using Common.Helper.Implementation;
using Common.Helper.Interface;
using Common.Models;
using Common.ViewModels;
using Microsoft.EntityFrameworkCore;
using Repositories.MyDbContext;
using Services.Interfaces;
using System.Globalization;

namespace Services.Implementation;

public class ChatSessionService : IChatSessionService
{
    private readonly MyDbContext _context;
    private readonly IUserHelper _jwtHelper;

    public ChatSessionService(MyDbContext context, IUserHelper jwtHelper)
    {
        _context = context;
        _jwtHelper = jwtHelper;
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
                SessionName = userLocalTime.ToString( "yyyy年MM月dd日HH時mm分", CultureInfo.InvariantCulture ),
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
                .Select(a => new ChatSessionViewModel ()
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

}
