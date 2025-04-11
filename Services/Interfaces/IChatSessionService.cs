using Common.Dto;
using Common.Params;

namespace Services.Interfaces;

public interface IChatSessionService
{
    /// <summary>
    /// 創建聊天會話
    /// </summary>
    /// <param name="userTimeZoneId"></param>
    /// <returns></returns>
    public Task<ResultDTO> GenerateChatSession(string userTimeZoneId = "Asia/Hong_Kong");

    /// <summary>
    /// 獲取該用戶的所有聊天會話
    /// </summary>
    /// <returns></returns>
    public Task<ResultDTO> GetChatSessionList();

    /// <summary>
    /// 測試 heartbeat
    /// </summary>
    /// <returns></returns>
    public Task<ResultDTO> CheackChatHttpClientHealth();

    /// <summary>
    /// 獲取對話歷史記錄
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public Task<ResultDTO> GetChatHistoryBySessionId(string sessionId);

    /// <summary>
    /// 刪除對話
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public Task<ResultDTO> DeleteChatData(string sessionId);

    /// <summary>
    /// 聊天功能
    /// </summary>
    /// <param name="chatParams"></param>
    /// <returns></returns>
    public Task<ResultDTO> Chat(ChatParams chatParams);

    /// <summary>
    ///  sse 聊天功能
    /// </summary>
    /// <param name="outputStream"></param>
    /// <param name="chatParams"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task ChatStream(Stream outputStream, ChatParams chatParams, CancellationToken cancellationToken);
}
