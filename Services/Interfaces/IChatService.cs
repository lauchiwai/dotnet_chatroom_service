using Common.Dto;
using Common.Params.Chat;

namespace Services.Interfaces;

public interface IChatService
{
    /// <summary>
    /// 創建聊天會話
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public Task<ResultDTO> GenerateChatSession(ChatSessionParams param);

    /// <summary>
    /// 創建 文章相關的聊天會話
    /// </summary>
    /// <param name="articleId"></param>
    /// <returns></returns>
    public Task<ResultDTO> GenerateRagChatSession(int articleId);

    /// <summary>
    /// 獲取該用戶的所有聊天會話
    /// </summary>
    /// <returns></returns>
    public Task<ResultDTO> GetChatSessionList();

    /// <summary>
    /// 獲取該用戶的所以場景對話
    /// </summary>
    /// <returns></returns>
    public Task<ResultDTO> GetSceneChatSessionList();

    /// <summary>
    /// 獲取該用戶的文章 RAG 對話
    /// </summary>
    /// <param name="articleId"></param>
    /// <returns></returns>
    public Task<ResultDTO> GetRagChatSessionListByArticleId(int articleId);

    /// <summary>
    /// 驗證 聊天室權限 
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public Task<ResultDTO> ValidateChatPermission(int sessionId);

    /// <summary>
    /// 獲取對話歷史記錄
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public Task<ResultDTO> GetChatHistory(int sessionId);

    /// <summary>
    /// 更新上一次使用這個聊天室的時間
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public Task<ResultDTO> RefreshChatSessionTime(int sessionId);

    /// <summary>
    /// 刪除對話
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public Task<ResultDTO> DeleteChatData(int sessionId);

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

    /// <summary>
    /// sse 場景對話
    /// </summary>
    /// <param name="outputStream"></param>
    /// <param name="sceneChatParams"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task SceneChatStream(Stream outputStream, SceneChatParams sceneChatParams, CancellationToken cancellationToken);
}
