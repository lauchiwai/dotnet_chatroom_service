using Common.Dto;
using Common.Params.Chat;

namespace Services.Interfaces;

public interface IChatService
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
    /// 驗證 聊天室權限 
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public Task<ResultDTO> ValidateChatPermission(string sessionId);

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
    public Task<ResultDTO> GetChatHistory(string sessionId);

    /// <summary>
    /// 更新上一次使用這個聊天室的時間
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public Task<ResultDTO> RefreshChatSessionTime(string sessionId);

    /// <summary>
    /// 刪除對話
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public Task<ResultDTO> DeleteChatData(string sessionId);

    /// <summary>
    ///  sse 聊天功能
    /// </summary>
    /// <param name="outputStream"></param>
    /// <param name="chatParams"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task ChatStream(Stream outputStream, ChatParams chatParams, CancellationToken cancellationToken);

    /// <summary>
    /// sse 獲取摘要
    /// </summary>
    /// <param name="outputStream"></param>
    /// <param name="summaryParams"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task SummaryStream(Stream outputStream, SummaryParams summaryParams, CancellationToken cancellationToken);
    
}
