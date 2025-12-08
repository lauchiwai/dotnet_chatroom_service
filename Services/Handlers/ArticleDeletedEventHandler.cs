using Common.Commands;
using Common.Dto;
using MediatR;
using Repositories.MyDbContext;
using Services.Interfaces;

public class ArticleDeletedEventHandler : IRequestHandler<DeleteArticleSession, ResultDTO>
{
    private readonly IArticleService _articleService;
    private readonly IChatService _chatService;
    private readonly MyDbContext _context;

    public ArticleDeletedEventHandler(
        IArticleService articleService,
        IChatService chatService,
        MyDbContext context)
    {
        _articleService = articleService;
        _chatService = chatService;
        _context = context;
    }

    public async Task<ResultDTO> Handle(DeleteArticleSession request, CancellationToken ct)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // 1. 刪除文章（使用外部事務）
            var articleResult = await _articleService.DeleteArticleWithTransaction(request.ArticleId, transaction);
            if (!articleResult.IsSuccess)
            {
                await transaction.RollbackAsync();
                return articleResult;
            }

            // 2. 刪除所有相關會話（使用同一個外部事務）
            var failedSessions = new List<int>();
            foreach (var sessionId in request.SessionIds)
            {
                var sessionResult = await _chatService.DeleteChatDataWithTransaction(sessionId, transaction);
                if (!sessionResult.IsSuccess)
                {
                    failedSessions.Add(sessionId);
                }
            }

            // 3. 如果有任何會話刪除失敗，回滾整個事務
            if (failedSessions.Any())
            {
                await transaction.RollbackAsync();
                return new ResultDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"刪除失敗，已回滾所有操作。失敗會話ID: {string.Join(", ", failedSessions)}"
                };
            }

            // 4. 所有操作成功，提交事務
            await transaction.CommitAsync();

            return new ResultDTO
            {
                IsSuccess = true,
                Code = 200,
                Message = "文章及所有關聯會話已成功刪除"
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            return new ResultDTO
            {
                IsSuccess = false,
                Code = 500,
                Message = $"刪除過程中發生錯誤: {ex.Message}"
            };
        }
    }
}