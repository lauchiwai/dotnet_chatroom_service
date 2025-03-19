using Common.Dto;

namespace Services.Interfaces;

public interface IChatSessionService
{
    /// <summary>
    /// 創建聊天會話
    /// </summary>
    /// <param name="userTimeZoneId"></param>
    /// <returns></returns>
    public Task<ResultDTO> GenerateChatSession(string userTimeZoneId = "Asia/Hong_Kong");
}
